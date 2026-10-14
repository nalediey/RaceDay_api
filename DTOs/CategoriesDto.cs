namespace RaceDay_api.DTOs
{
    public class CreateCategoryDto
    {
        public string CategoryName { get; set; }
        public string Distance { get; set; }
        public decimal EntryFee { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string CategoryName { get; set; }
        public string Distance { get; set; }
        public decimal EntryFee { get; set; }
    }
}
