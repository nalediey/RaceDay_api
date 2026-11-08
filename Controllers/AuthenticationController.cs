using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay_api.Database;
using RaceDay_api.DTOs;
using RaceDay_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace RaceDay_api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration; 

        public AuthenticationController(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser(RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.Password != dto.Comfirmpassword)
                return BadRequest("Passwords do not match.");

            var role = dto.role.Trim();
            if (!role.Equals("Organiser", StringComparison.OrdinalIgnoreCase) &&
                !role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid role. Role must be either 'Organiser' or 'Participant'.");
            }

            var email = dto.Email.Trim().ToLowerInvariant();
            var emailExists = await _context.Users.AnyAsync(u => u.Email == email);
            if (emailExists) return BadRequest("Email already exists.");

            var user = new RegisterUser
            {
                first_name = dto.first_name,
                last_name = dto.last_name,
                Email = email,
                // HASH THE PASSWORD 
                password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                role = role,
                PhoneNUmber = dto.PhoneNUmber
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();


            return StatusCode(201, new
            {
                message = "User registered successfully.",
                R_user_id = user.R_user_id
            });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var email = dto.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return Unauthorized("Invalid email or password.");

            // VERIFY THE HASHED PASSWORD HERE
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.password))
            {
                return Unauthorized("Invalid email or password.");
            }

            // Generate JWT Token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful.",
                token = token,
                R_user_id = user.R_user_id,
                firstName = user.first_name,
                lastName = user.last_name,
                email = user.Email,
                role = user.role
            });
        }

        //  method to generate the JWT
        private string GenerateJwtToken(RegisterUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.R_user_id.ToString()),
                new Claim(ClaimTypes.Role, user.role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
