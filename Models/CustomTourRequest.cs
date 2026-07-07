using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelProject.Models
{
    public class CustomTourRequest
    {
        [Key]
        public int RequestID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(200)]
        public string Destinations { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; }

        [Required]
        public int DurationDays { get; set; }

        [Required]
        public int AdultCount { get; set; }

        public int ChildCount { get; set; }

        [StringLength(100)]
        public string? HotelStandard { get; set; }

        [StringLength(100)]
        public string? TransportType { get; set; }

        public string? Note { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? QuotedPrice { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Quoted, Accepted, Rejected

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("UserID")]
        public User? User { get; set; }
    }
}
