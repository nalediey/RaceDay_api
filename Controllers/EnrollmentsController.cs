using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_api.Database;
using RaceDay_api.DTOs;
using RaceDay_api.Models;
using System.Security.Claims;

namespace RaceDay_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EnrollmentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // POST /api/events/{eventId}/enrolments
        [Authorize(Roles = "Participant")]
        [HttpPost("events/{eventId}/enrolments")]
        public async Task<IActionResult> CreateEnrolment(
            int eventId,
            [FromBody] CreateEnrolmentDto dto)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id == dto.CategoryID &&
                    c.Id == eventId);

            if (category == null)
                return NotFound(
                    "Category not found for this event.");

            // Prevent duplicate enrolment in the same category.
            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantID == int.Parse(userId) &&
                    e.CategoryID == dto.CategoryID);

            if (alreadyEnrolled)
                return Conflict("Already enrolled in this category.");

            var enrolment = new Enrolment
            {
                ParticipantID = int.Parse(userId),
                CategoryID = dto.CategoryID,
                EntryDate = DateTime.UtcNow,
                Status = "Registered"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Created(
                $"/api/events/{eventId}/enrolments",
                new
                {
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

            if (userId == null)
                return Unauthorized();

            var participantId = int.Parse(userId);

            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantID == participantId)
                .Include(e => e.Category)
                    .ThenInclude(c => c.Event)
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.EntryDate,
                    e.Status,
                    e.CategoryID,
                    CategoryName = e.Category!.CategoryName,
                    EventName = e.Category.Event!.Name
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
            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Where(e => e.Category!.Id == eventId)
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.ParticipantID,
                    ParticipantName =
                        e.Participant!.first_name + " " +
                        e.Participant.last_name,
                    e.CategoryID,
                    CategoryName = e.Category!.CategoryName,
                    e.EntryDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}
