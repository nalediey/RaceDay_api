using System.ComponentModel.DataAnnotations;

namespace RaceDay_api.Models
{
    public class Enrolment
    {
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }
        public int CategoryID { get; set; }

        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Registered";

        public RegisterUser? Participant { get; set; }
        public Category? Category { get; set; }

        public Results? Result { get; set; }
    }


}
       
   