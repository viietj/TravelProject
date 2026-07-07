using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelProject.Models;
using Microsoft.AspNetCore.Http;

namespace TravelProject.Controllers
{
    public class HotelController : Controller
    {
        private readonly TravelDbContext _db;

        public HotelController(TravelDbContext db)
        {
            _db = db;
        }

        // GET: /Hotel
        public IActionResult Index(string? search, string? type, byte? rating, decimal? maxPrice)
        {
            var query = _db.Hotels.Include(h => h.Destination).Where(h => h.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(h => h.Name.Contains(search) || 
                                     h.City.Contains(search) || 
                                     h.Address.Contains(search) ||
                                     (h.Destination != null && h.Destination.Name.Contains(search)));
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(h => h.HotelType == type);
            }

            if (rating.HasValue && rating.Value > 0)
            {
                query = query.Where(h => h.StarRating == rating.Value);
            }

            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                query = query.Where(h => h.PricePerNight <= maxPrice.Value);
            }

            var hotels = query.OrderByDescending(h => h.StarRating)
                              .ThenBy(h => h.PricePerNight)
                              .ToList();

            ViewBag.Search = search;
            ViewBag.Type = type;
            ViewBag.Rating = rating;
            ViewBag.MaxPrice = maxPrice;

            return View(hotels);
        }

        // GET: /Hotel/Details/5
        public IActionResult Details(int id)
        {
            var hotel = _db.Hotels.Include(h => h.Destination)
                                  .FirstOrDefault(h => h.HotelID == id);

            if (hotel == null || !hotel.IsActive)
            {
                return NotFound();
            }

            return View(hotel);
        }

        // GET: /Hotel/Book?hotelId=5
        public IActionResult Book(int hotelId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                TempData["Error"] = "Please login to book a hotel.";
                return RedirectToAction("Login", "Account");
            }

            var hotel = _db.Hotels.FirstOrDefault(h => h.HotelID == hotelId);
            if (hotel == null || !hotel.IsActive)
            {
                return NotFound();
            }

            // Default dates
            var checkIn = DateTime.Today.AddDays(1);
            var checkOut = DateTime.Today.AddDays(2);
            string roomType = "Standard";

            int bookedRooms = _db.HotelBookings
                .Where(b => b.HotelID == hotelId 
                         && b.RoomType == roomType 
                         && b.Status != "Cancelled"
                         && b.CheckInDate < checkOut 
                         && b.CheckOutDate > checkIn)
                .Sum(b => b.RoomCount);

            int maxRooms = 10; // Standard capacity
            ViewBag.AvailableRooms = Math.Max(0, maxRooms - bookedRooms);

            var user = _db.Users.Find(userId);

            ViewBag.Hotel = hotel;
            ViewBag.CustomerName = user?.FullName ?? user?.Username ?? "";
            ViewBag.CustomerPhone = user?.Phone ?? "";
            ViewBag.CustomerEmail = user?.Email ?? "";

            return View();
        }

        // GET: /Hotel/GetAvailableRooms?hotelId=5&roomType=Standard&checkIn=2026-06-18&checkOut=2026-06-19
        [HttpGet]
        public IActionResult GetAvailableRooms(int hotelId, string roomType, DateTime checkIn, DateTime checkOut)
        {
            if (checkIn >= checkOut || checkIn < DateTime.Today)
            {
                return Json(new { availableRooms = 0 });
            }

            int bookedRooms = _db.HotelBookings
                .Where(b => b.HotelID == hotelId 
                         && b.RoomType == roomType 
                         && b.Status != "Cancelled"
                         && b.CheckInDate < checkOut 
                         && b.CheckOutDate > checkIn)
                .Sum(b => b.RoomCount);

            int maxRooms = roomType switch
            {
                "Deluxe" => 5,
                "Suite" => 2,
                _ => 10 // Standard
            };

            int availableRooms = Math.Max(0, maxRooms - bookedRooms);
            return Json(new { availableRooms });
        }

        // POST: /Hotel/Book
        [HttpPost]
        public IActionResult Book(int hotelId, string customerName, string customerPhone, 
            string customerEmail, DateTime checkInDate, DateTime checkOutDate, 
            int adultCount, int childCount, int roomCount, string roomType, string? notes)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                TempData["Error"] = "Please login to book a hotel.";
                return RedirectToAction("Login", "Account");
            }

            var hotel = _db.Hotels.FirstOrDefault(h => h.HotelID == hotelId);
            if (hotel == null || !hotel.IsActive)
            {
                return NotFound();
            }

            // Validate stay dates
            if (checkInDate >= checkOutDate)
            {
                TempData["Error"] = "Check-out date must be after check-in date.";
                PopulateHotelErrorViewBag(hotel, customerName, customerPhone, customerEmail, checkInDate, checkOutDate, adultCount, childCount, roomCount, roomType, notes);
                return View();
            }

            if (checkInDate < DateTime.Today)
            {
                TempData["Error"] = "Check-in date cannot be in the past.";
                PopulateHotelErrorViewBag(hotel, customerName, customerPhone, customerEmail, checkInDate, checkOutDate, adultCount, childCount, roomCount, roomType, notes);
                return View();
            }

            // Validate travelers and rooms count
            if (adultCount < 1)
            {
                TempData["Error"] = "There must be at least 1 adult for reservation.";
                PopulateHotelErrorViewBag(hotel, customerName, customerPhone, customerEmail, checkInDate, checkOutDate, adultCount, childCount, roomCount, roomType, notes);
                return View();
            }

            if (roomCount < 1)
            {
                TempData["Error"] = "You must reserve at least 1 room.";
                PopulateHotelErrorViewBag(hotel, customerName, customerPhone, customerEmail, checkInDate, checkOutDate, adultCount, childCount, roomCount, roomType, notes);
                return View();
            }

            // Double submit check (identical reservation in last 30 seconds)
            var duplicate = _db.HotelBookings
                .Where(b => b.UserID == userId 
                         && b.HotelID == hotelId 
                         && b.CheckInDate == checkInDate 
                         && b.CheckOutDate == checkOutDate 
                         && b.RoomType == roomType 
                         && b.Status != "Cancelled")
                .OrderByDescending(b => b.CreatedDate)
                .FirstOrDefault();

            if (duplicate != null && (DateTime.Now - duplicate.CreatedDate).TotalSeconds < 30)
            {
                TempData["Error"] = "You have already submitted an identical reservation request recently. Please wait a moment.";
                PopulateHotelErrorViewBag(hotel, customerName, customerPhone, customerEmail, checkInDate, checkOutDate, adultCount, childCount, roomCount, roomType, notes);
                return View();
            }

            // Calculate max capacity and booked rooms
            int maxRooms = roomType switch
            {
                "Deluxe" => 5,
                "Suite" => 2,
                _ => 10 // Standard
            };

            int bookedRooms = _db.HotelBookings
                .Where(b => b.HotelID == hotelId 
                         && b.RoomType == roomType 
                         && b.Status != "Cancelled"
                         && b.CheckInDate < checkOutDate 
                         && b.CheckOutDate > checkInDate)
                .Sum(b => b.RoomCount);

            if (bookedRooms + roomCount > maxRooms)
            {
                TempData["Error"] = $"Not enough rooms available! Only {Math.Max(0, maxRooms - bookedRooms)} rooms of type '{roomType}' left for the selected dates.";
                PopulateHotelErrorViewBag(hotel, customerName, customerPhone, customerEmail, checkInDate, checkOutDate, adultCount, childCount, roomCount, roomType, notes);
                return View();
            }

            // Calculate nights
            int nights = (checkOutDate.Date - checkInDate.Date).Days;
            
            // Basic room type price multiplier
            decimal roomMultiplier = roomType switch
            {
                "Deluxe" => 1.3m,
                "Suite" => 1.8m,
                _ => 1.0m // Standard
            };

            decimal totalPrice = hotel.PricePerNight * roomCount * nights * roomMultiplier;

            var booking = new HotelBooking
            {
                HotelID = hotelId,
                UserID = userId.Value,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                CustomerEmail = customerEmail,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                AdultCount = adultCount,
                ChildCount = childCount,
                RoomCount = roomCount,
                RoomType = roomType,
                TotalPrice = totalPrice,
                Status = "Pending",
                PaymentStatus = "Unpaid",
                Notes = notes,
                CreatedDate = DateTime.Now
            };

            _db.HotelBookings.Add(booking);
            _db.SaveChanges();

            TempData["Success"] = $"Successfully booked a room at {hotel.Name}! Please proceed to payment to confirm your booking.";
            return RedirectToAction("MyBookings", "Booking");
        }

        private void PopulateHotelErrorViewBag(Hotel hotel, string customerName, string customerPhone, string customerEmail, DateTime checkInDate, DateTime checkOutDate, int adultCount, int childCount, int roomCount, string roomType, string? notes)
        {
            int bookedRooms = 0;
            if (checkInDate < checkOutDate && checkInDate >= DateTime.Today)
            {
                bookedRooms = _db.HotelBookings
                    .Where(b => b.HotelID == hotel.HotelID 
                             && b.RoomType == roomType 
                             && b.Status != "Cancelled"
                             && b.CheckInDate < checkOutDate 
                             && b.CheckOutDate > checkInDate)
                    .Sum(b => b.RoomCount);
            }

            int maxRooms = roomType switch
            {
                "Deluxe" => 5,
                "Suite" => 2,
                _ => 10 // Standard
            };

            ViewBag.Hotel = hotel;
            ViewBag.AvailableRooms = Math.Max(0, maxRooms - bookedRooms);
            ViewBag.CustomerName = customerName;
            ViewBag.CustomerPhone = customerPhone;
            ViewBag.CustomerEmail = customerEmail;
            ViewBag.CheckInDate = checkInDate.ToString("yyyy-MM-dd");
            ViewBag.CheckOutDate = checkOutDate.ToString("yyyy-MM-dd");
            ViewBag.AdultCount = adultCount;
            ViewBag.ChildCount = childCount;
            ViewBag.RoomCount = roomCount;
            ViewBag.RoomType = roomType;
            ViewBag.Notes = notes;
        }

        // POST: /Hotel/CancelBooking
        [HttpPost]
        public IActionResult CancelBooking(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = _db.HotelBookings.FirstOrDefault(b => b.BookingID == bookingId && b.UserID == userId.Value);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status != "Pending")
            {
                TempData["Error"] = "Only bookings in 'Pending' status can be cancelled.";
                return RedirectToAction("MyBookings", "Booking");
            }

            booking.Status = "Cancelled";
            _db.SaveChanges();

            TempData["Success"] = "Your hotel reservation has been cancelled.";
            return RedirectToAction("MyBookings", "Booking");
        }
    }
}