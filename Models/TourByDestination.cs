namespace TravelProject.Models
{
    public class TourByDestination
    {
        public int TourID { get; set; }
        public string? Title { get; set; }
        public string? Region { get; set; }
        public string? TourType { get; set; }
        public int DurationDays { get; set; }
        public decimal PricePerPerson { get; set; }
        public string? ImageUrl { get; set; }
        public int DestinationID { get; set; }
        public string? DestinationName { get; set; }
        public string? DestinationCity { get; set; }
        public string? DestinationType { get; set; }
    }
}