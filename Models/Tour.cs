using System;

namespace TravelProject.Models
{
    public class Tour
    {
        public int TourID { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? MainDestinationID { get; set; }
        public string? Region { get; set; }
        public string? TourType { get; set; }
        public int DurationDays { get; set; }
        public int DurationNights { get; set; }
        public int? MaxGroupSize { get; set; }
        public decimal PricePerPerson { get; set; }
        public decimal PricePerChild { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? DepartureCity { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}