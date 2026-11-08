namespace RaceDay_api.DTOs
{
    // Used for POST /api/events
    public class CreateEventDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Distance { get; set; }
        public string EventType { get; set; } // e.g., "run", "walk", "cycle"

        public IFormFile? BannerImage { get; set; }
    }

    // Used for PUT /api/events/{id}
    public class UpdateEventDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Distance { get; set; }
        public string EventType { get; set; }
    }
}
