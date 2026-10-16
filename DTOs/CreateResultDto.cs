using System.ComponentModel.DataAnnotations;

namespace RaceDay_api.DTOs
{
    public class CreateResultDto
    {
        [Required]
        public TimeSpan FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int Position { get; set; }
    }
}