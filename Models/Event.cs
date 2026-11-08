namespace RaceDay_api.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Distance { get; set; }
        public string EventType { get; set; } = string.Empty;

        // Foreign Key to Organiser (User)
        public int OrganiserId { get; set; }
        public RegisterUser Organiser { get; set; } = null!;

        public ICollection<Category> Categories { get; set; } = new List<Category>();


        public string? BannerImageUrl { get; set; }
    }
}
