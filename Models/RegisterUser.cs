using System.ComponentModel.DataAnnotations;

namespace RaceDay_api.Models
{
    public class RegisterUser
    {
        [Key]
        public int R_user_id { get; set; }

        [Required]
        public string first_name { get; set; } = string.Empty;

        [Required]
        public string last_name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;

        [Required]
        public string role { get; set; } = string.Empty;

        [Required]
        public string PhonuNUmber { get; set; } = string.Empty;
    }
}

