public class TourViewModel
{
    public int TourId { get; set; }
    public string? Title{ get; set; }
    public string? Region { get; set; }
    public string? TourType { get; set; }
    public int DurationDays { get; set; }
    public int DurationNights { get; set; }
    public decimal PricePerPerson { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string? DepartureCity { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public string? MainDestinationName { get; set; }
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public DateTime? NextDeparture { get; set; }
    public int TotalAvailableSlots { get; set; }
}