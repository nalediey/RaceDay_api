using System.ComponentModel.DataAnnotations;

namespace RaceDay_api.Models
{
    public class Enrollments
    {
        [Key]
        public int EnrolmentID { get; set; }
        [Required]
        public int ParticipantID { get; set; }
        [Required]
        public int CategoryID { get; set; }
        [Required]
        public int EntryDate { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
    }
     
    
}
       
   