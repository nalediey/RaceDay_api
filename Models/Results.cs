using System.ComponentModel.DataAnnotations;

namespace RaceDay_api.Models
{
    public class Results
    {
        [Key]
        public int ResultID { get; set; }

        public int EnrolmentID { get; set; }

        public TimeSpan FinishTime { get; set; }

        public int Position { get; set; }

        public Enrolment? Enrolment { get; set; }
    }
}
