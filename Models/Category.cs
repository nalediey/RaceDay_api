namespace RaceDay_api.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public String Distance { get; set; }
        public decimal EntryFee { get; set; }

        // Foreign Key to Event
        public int EventId { get; set; }

    }
}
