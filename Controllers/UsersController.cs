using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_api.Database;
using RaceDay_api.DTOs;
using System.Security.Claims;

namespace RaceDay_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // This ensures BOTH Organisers and Participants must be logged in
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public UsersController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: /api/users/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Getting the user ID from the JWT token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Invalid token.");
            }

            //Finding the user in the database
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Returning user the profile 
            return Ok(new
            {
                R_user_id = user.R_user_id,
                FirstName = user.first_name,
                LastName = user.last_name,
                Email = user.Email,
                Role = user.role,
                PhoneNumber = user.PhoneNUmber
            });
        }

        // PUT: /api/users/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Get the user ID from the JWT token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("Invalid token.");
            }

            // Find the user
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");

            // Check if they are changing their email, and if the new email is already taken
            var newEmail = dto.Email.Trim().ToLowerInvariant();
            if (user.Email != newEmail)
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == newEmail && u.R_user_id != userId);

                if (emailExists)
                {
                    return BadRequest("Email is already in use by another account.");
                }
                user.Email = newEmail;
            }

            //Update the allowed fields
            user.first_name = dto.FirstName;
            user.last_name = dto.LastName;

            //Save changes
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                FirstName = user.first_name,
                LastName = user.last_name,
                Email = user.Email
            });
        }
    }
}
