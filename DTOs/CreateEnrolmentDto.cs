using System.ComponentModel.DataAnnotations;

namespace RaceDay_api.DTOs
{
    public class CreateEnrolmentDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }
    }
}