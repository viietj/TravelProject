using System;

namespace TravelProject.Models
{
    public class HotelByDestination
    {
        public int HotelID { get; set; }
        public string HotelName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public byte StarRating { get; set; }
        public string HotelType { get; set; } = null!;
        public decimal PricePerNight { get; set; }
        public string? Amenities { get; set; }
        public string? ImageUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public int DestinationID { get; set; }
        public string DestinationName { get; set; } = null!;
        public string Region { get; set; } = null!;
        public string DestinationType { get; set; } = null!;
    }
}
