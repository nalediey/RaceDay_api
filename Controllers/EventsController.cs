using Microsoft.AspNetCore.Authorization;
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
        private readonly IWebHostEnvironment _env; // Added for image saving

        public EventsController(ApplicationDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
        public async Task<ActionResult<Event>> CreateEvent([FromForm] CreateEventDto dto) // Changed to FromForm
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int organiserId))
            {
                return Unauthorized("Invalid token.");
            }

            string imageUrl = null;

            // Handle the file upload
            if (dto.BannerImage != null && dto.BannerImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.BannerImage.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.BannerImage.CopyToAsync(stream);
                }

                imageUrl = $"/images/{uniqueFileName}";
            }

            var newEvent = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                Date = dto.Date,
                Location = dto.Location,
                Distance = dto.Distance,
                EventType = dto.EventType,
                OrganiserId = organiserId,
                BannerImageUrl = imageUrl // Save the URL to the DB
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
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int organiserId))
            {
                return Unauthorized("Invalid token.");
            }

            var existingEvent = await _context.Events.FindAsync(id);

            if (existingEvent == null) return NotFound();
            if (existingEvent.OrganiserId != organiserId)
                return Forbid("You can only edit your own events.");

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