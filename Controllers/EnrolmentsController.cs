using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_api.Database;
using RaceDay_api.DTOs;
using RaceDay_api.Models;
using System.Security.Claims;

namespace RaceDay_api.Controllers
{
    [Route("api")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EnrolmentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // POST /api/events/{eventId}/enrolments
        [Authorize(Roles = "Participant")]
        [HttpPost("events/{eventId}/enrolments")]
        public async Task<IActionResult> CreateEnrolment(
            int eventId, CreateEnrolmentDto dto)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int participantId))
                return Unauthorized();

            var eventExists = await _context.Events
                .AnyAsync(e => e.Id == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id == dto.CategoryID &&
                    c.EventId == eventId);

            if (category == null)
                return NotFound(
                    "Category does not belong to this event.");

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantID == participantId &&
                    e.CategoryID == dto.CategoryID);

            if (alreadyEnrolled)
                return Conflict(
                    "You are already enrolled in this category.");

            var enrolment = new Enrolment
            {
                ParticipantID = participantId,
                CategoryID = dto.CategoryID,
                EntryDate = DateTime.UtcNow,
                Status = "Registered"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Enrolment created successfully.",
                enrolment.EnrolmentID,
                enrolment.ParticipantID,
                enrolment.CategoryID,
                enrolment.EntryDate,
                enrolment.Status
            });
        }

        // GET /api/users/enrolments
        [Authorize(Roles = "Participant")]
        [HttpGet("users/enrolments")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int participantId))
                return Unauthorized();

            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantID == participantId)
                .Join(
                    _context.Categories,
                    enrolment => enrolment.CategoryID,
                    category => category.Id,
                    (enrolment, category) => new
                    {
                        enrolment,
                        category
                    })
                .Join(
                    _context.Events,
                    item => item.category.EventId,
                    ev => ev.Id,
                    (item, ev) => new
                    {
                        item.enrolment,
                        item.category,
                        EventName = ev.Name,
                        EventDate = ev.Date,
                        EventLocation = ev.Location
                    })
                .Select(x => new
                {
                    x.enrolment.EnrolmentID,
                    x.enrolment.EntryDate,
                    x.enrolment.Status,
                    CategoryID = x.category.Id,
                    x.category.CategoryName,
                    x.EventName,
                    x.EventDate,
                    x.EventLocation
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET /api/events/{eventId}/enrolments
        [Authorize(Roles = "Organiser")]
        [HttpGet("events/{eventId}/enrolments")]
        public async Task<IActionResult> GetEventEnrolments(
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

            var enrolments = await _context.Enrolments
                .Join(
                    _context.Categories,
                    enrolment => enrolment.CategoryID,
                    category => category.Id,
                    (enrolment, category) => new
                    {
                        enrolment,
                        category
                    })
                .Where(x => x.category.EventId == eventId)
                .Join(
                    _context.Users,
                    x => x.enrolment.ParticipantID,
                    user => user.R_user_id,
                    (x, user) => new
                    {
                        x.enrolment.EnrolmentID,
                        x.enrolment.ParticipantID,
                        ParticipantName =
                            user.first_name + " " + user.last_name,
                        x.category.Id,
                        x.category.CategoryName,
                        x.enrolment.EntryDate,
                        x.enrolment.Status
                    })
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}