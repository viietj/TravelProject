using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelProject.Models
{
    public class Hotel
    {
        [Key]
        public int HotelID { get; set; }

        [Required]
        public int DestinationID { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string Address { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = null!;

        [Required]
        [Range(1, 5)]
        public byte StarRating { get; set; }

        [Required]
        [StringLength(50)]
        public string HotelType { get; set; } = "Hotel";

        [Required]
        [Column(TypeName = "decimal(12, 0)")]
        public decimal PricePerNight { get; set; }

        public string? Description { get; set; }

        [StringLength(500)]
        public string? Amenities { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Website { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("DestinationID")]
        public Destination? Destination { get; set; }
    }
}
