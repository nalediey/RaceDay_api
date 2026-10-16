using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResultsModel = RaceDay_api.Models.Results;
using RaceDay_api.Database;
using RaceDay_api.DTOs;
using System.Security.Claims;

namespace RaceDay_api.Controllers
{
    [Route("api")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ResultsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // POST /api/enrolments/{enrolmentId}/result
        [Authorize(Roles = "Organiser")]
        [HttpPost("enrolments/{enrolmentId}/result")]
        public async Task<IActionResult> RecordResult(
            int enrolmentId, CreateResultDto dto)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
                return Unauthorized();

            if (dto.FinishTime <= TimeSpan.Zero)
                return BadRequest(
                    "Finish time must be greater than zero.");

            if (dto.Position <= 0)
                return BadRequest(
                    "Position must be greater than zero.");

            var enrolment = await _context.Enrolments
                .Join(
                    _context.Categories,
                    e => e.CategoryID,
                    c => c.Id,
                    (e, c) => new
                    {
                        Enrolment = e,
                        c.EventId
                    })
                .Join(
                    _context.Events,
                    x => x.EventId,
                    ev => ev.Id,
                    (x, ev) => new
                    {
                        x.Enrolment,
                        ev.OrganiserId
                    })
                .FirstOrDefaultAsync(x =>
                    x.Enrolment.EnrolmentID == enrolmentId &&
                    x.OrganiserId == organiserId);

            if (enrolment == null)
                return NotFound(
                    "Enrolment not found for your events.");

            var resultExists = await _context.results
                .AnyAsync(r => r.EnrolmentID == enrolmentId);

            if (resultExists)
                return Conflict(
                    "A result has already been recorded.");

            var result = new ResultsModel
            {
                EnrolmentID = enrolmentId,
                FinishTime = dto.FinishTime,
                Position = dto.Position
            };
            _context.results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Result recorded successfully.",
                result.ResultID,
                result.EnrolmentID,
                result.FinishTime,
                result.Position
            });
        }

        // GET /api/users/results
        [Authorize(Roles = "Participant")]
        [HttpGet("users/results")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int participantId))
                return Unauthorized();

            var results = await _context.results
                .Join(
                    _context.Enrolments,
                    result => result.EnrolmentID,
                    enrolment => enrolment.EnrolmentID,
                    (result, enrolment) => new
                    {
                        result,
                        enrolment
                    })
                .Where(x =>
                    x.enrolment.ParticipantID == participantId)
                .Join(
                    _context.Categories,
                    x => x.enrolment.CategoryID,
                    category => category.Id,
                    (x, category) => new
                    {
                        x.result,
                        category.EventId,
                        category.CategoryName
                    })
                .Join(
                    _context.Events,
                    x => x.EventId,
                    ev => ev.Id,
                    (x, ev) => new
                    {
                        x.result.ResultID,
                        x.result.EnrolmentID,
                        EventName = ev.Name,
                        x.CategoryName,
                        x.result.FinishTime,
                        x.result.Position
                    })
                .OrderBy(x => x.Position)
                .ToListAsync();

            return Ok(results);
        }

        // GET /api/events/{eventId}/results
        [Authorize(Roles = "Organiser")]
        [HttpGet("events/{eventId}/results")]
        public async Task<IActionResult> GetEventResults(
            int eventId)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
                return Unauthorized();

            var ownsEvent = await _context.Events
                .AnyAsync(e =>
                    e.Id == eventId &&
                    e.OrganiserId == organiserId);

            if (!ownsEvent)
                return NotFound("Event not found.");

            var results = await _context.results
                .Join(
                    _context.Enrolments,
                    result => result.EnrolmentID,
                    enrolment => enrolment.EnrolmentID,
                    (result, enrolment) => new
                    {
                        result,
                        enrolment
                    })
                .Join(
                    _context.Categories,
                    x => x.enrolment.CategoryID,
                    category => category.Id,
                    (x, category) => new
                    {
                        x.result,
                        x.enrolment,
                        category.EventId,
                        category.CategoryName
                    })
                .Where(x => x.EventId == eventId)
                .Join(
                    _context.Users,
                    x => x.enrolment.ParticipantID,
                    user => user.R_user_id,
                    (x, user) => new
                    {
                        x.result.ResultID,
                        x.result.EnrolmentID,
                        ParticipantName =
                            user.first_name + " " + user.last_name,
                        x.CategoryName,
                        x.result.FinishTime,
                        x.result.Position
                    })
                .OrderBy(x => x.Position)
                .ToListAsync();

            return Ok(results);
        }
    }
}