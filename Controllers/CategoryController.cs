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
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public CategoryController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/events/{eventId}/categories
        [HttpGet("events/{eventId}/categories")]
        [Authorize(Roles = "Organiser,Participant")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists) return NotFound("Event not found.");

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/events/{eventId}/categories
        [HttpPost("events/{eventId}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Category>> CreateCategory(int eventId, [FromBody] CreateCategoryDto dto)
        {
            //Get Organiser ID
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int organiserId)) return Unauthorized("Invalid token.");

            //Check if event exists AND belongs to this Organiser
            var existingEvent = await _context.Events.FindAsync(eventId);
            if (existingEvent == null) return NotFound("Event not found.");
            if (existingEvent.OrganiserId != organiserId)
                return Forbid("You can only add categories to your own events.");

            //Create the category
            var newCategory = new Category
            {
                CategoryName = dto.CategoryName,
                Distance = dto.Distance,
                EntryFee = dto.EntryFee,
                EventId = eventId
            };

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return StatusCode(201, newCategory);
        }

        // PUT: api/categories/{id}
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int organiserId)) return Unauthorized("Invalid token.");

            var category = await _context.Categories

                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound("Category not found.");

            // Verify ownership via the Event
            var parentEvent = await _context.Events.FindAsync(category.EventId);
            if (parentEvent == null || parentEvent.OrganiserId != organiserId)
                return Forbid("You can only edit categories for your own events.");

            // Update fields
            category.CategoryName = dto.CategoryName;
            category.Distance = dto.Distance;
            category.EntryFee = dto.EntryFee;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // DELETE: api/categories/{id}
        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int organiserId)) return Unauthorized("Invalid token.");

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound("Category not found.");

            // Verify ownership via the Event
            var parentEvent = await _context.Events.FindAsync(category.EventId);
            if (parentEvent == null || parentEvent.OrganiserId != organiserId)
                return Forbid("You can only delete categories for your own events.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}