using System.ComponentModel.DataAnnotations;

namespace RaceDay_api.DTOs
{
    public class RegisterDto
    {

        [Required]
        public string first_name { get; set; } = string.Empty;

        [Required]
        public string last_name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]

        public string Password { get; set; } = string.Empty;

        [Required]
        public string role { get; set; } = string.Empty;

        [Required]
        public string Comfirmpassword { get; set; } = string.Empty;

        [Required]
        public string PhoneNUmber { get; set; } = string.Empty;
    }
}
