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
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EventsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/events
        [HttpGet]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events.ToListAsync();
        }

        // GET: api/events/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();
            return @event;
        }

        // POST: api/events
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] CreateEventDto dto)
        {
            // Safely get the logged-in user's ID from JWT
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int organiserId))
            {
                return Unauthorized("Invalid token.");
            }

            // Map the DTO to the actual Database Model
            var newEvent = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                Date = dto.Date,
                Location = dto.Location,
                Distance = dto.Distance,
                EventType = dto.EventType,
                OrganiserId = organiserId
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.Id }, newEvent);
        }

        // PUT: api/events/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            // Safely get the logged-in user's ID
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int organiserId))
            {
                return Unauthorized("Invalid token.");
            }

            //Find the existing event
            var existingEvent = await _context.Events.FindAsync(id);

            //Check if it exists AND if they own it
            if (existingEvent == null) return NotFound();
            if (existingEvent.OrganiserId != organiserId)
                return Forbid("You can only edit your own events.");

            //Update only the allowed fields
            existingEvent.Name = dto.Name;
            existingEvent.Description = dto.Description;
            existingEvent.Date = dto.Date;
            existingEvent.Location = dto.Location;
            existingEvent.Distance = dto.Distance;
            existingEvent.EventType = dto.EventType;

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        // DELETE: api/events/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int organiserId))
            {
                return Unauthorized("Invalid token.");
            }

            var @event = await _context.Events.FindAsync(id);

            if (@event == null) return NotFound();
            if (@event.OrganiserId != organiserId) return Forbid("You can only delete your own events.");

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return NoContent(); 
        }
    }
}