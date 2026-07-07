using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelProject.Models
{
    public class HotelBooking
    {
        [Key]
        public int BookingID { get; set; }

        [Required]
        public int HotelID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = null!;

        [Required]
        [StringLength(15)]
        public string CustomerPhone { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string CustomerEmail { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, 100)]
        public int AdultCount { get; set; }

        [Required]
        [Range(0, 100)]
        public int ChildCount { get; set; }

        [Required]
        [Range(1, 20)]
        public int RoomCount { get; set; } = 1;

        [Required]
        [StringLength(100)]
        public string RoomType { get; set; } = "Standard";

        [Required]
        [Column(TypeName = "decimal(12, 0)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid, Pending

        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("HotelID")]
        public Hotel? Hotel { get; set; }

        [ForeignKey("UserID")]
        public User? User { get; set; }
    }
}
