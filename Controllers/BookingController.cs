using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelProject.Models;

namespace TravelProject.Controllers
{
    public class BookingController : Controller
    {
        private readonly TravelDbContext _db;
        private readonly TravelProject.Services.VnpayService _vnpay;
        
        public BookingController(TravelDbContext db, TravelProject.Services.VnpayService vnpay)
        {
            _db = db;
            _vnpay = vnpay;
        }

        // ================= ĐẶT TOUR CÓ SẴN =================

        // GET: /Booking/Create?tourId=5
        public IActionResult Create(int tourId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            var tour = _db.Tours.Find(tourId);
            if (tour == null || !tour.IsActive) return NotFound();

            // Default departure date to tomorrow
            var defaultDate = DateTime.Today.AddDays(1);
            ViewBag.DefaultDate = defaultDate.ToString("yyyy-MM-dd");

            // Calculate slots for the default date
            int bookedSlots = _db.Bookings
                .Where(b => b.TourID == tourId 
                         && b.DepartureDate.Date == defaultDate.Date 
                         && b.Status != "Cancelled")
                .Sum(b => (int?)(b.AdultCount + b.ChildCount)) ?? 0;

            int availableSlots = (tour.MaxGroupSize ?? 0) - bookedSlots;

            var user = _db.Users.Find(userId);

            ViewBag.Tour = tour;
            ViewBag.AvailableSlots = Math.Max(0, availableSlots);
            ViewBag.CustomerName  = user?.FullName ?? user?.Username ?? "";
            ViewBag.CustomerPhone = user?.Phone ?? "";
            ViewBag.CustomerEmail = user?.Email ?? "";

            return View();
        }

        // GET: /Booking/GetAvailableSlots?tourId=5&date=2026-06-18
        [HttpGet]
        public IActionResult GetAvailableSlots(int tourId, DateTime date)
        {
            var tour = _db.Tours.Find(tourId);
            if (tour == null) return NotFound();

            int bookedSlots = _db.Bookings
                .Where(b => b.TourID == tourId 
                         && b.DepartureDate.Date == date.Date 
                         && b.Status != "Cancelled")
                .Sum(b => (int?)(b.AdultCount + b.ChildCount)) ?? 0;

            int availableSlots = (tour.MaxGroupSize ?? 0) - bookedSlots;
            return Json(new { availableSlots = Math.Max(0, availableSlots) });
        }

        // POST: /Booking/Create
        [HttpPost]
        public IActionResult Create(int tourId, string customerName, string customerPhone,
            string customerEmail, DateTime departureDate, int adultCount, int childCount, string? notes)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            var tour = _db.Tours.Find(tourId);
            if (tour == null) return NotFound();

            // Validate departure date is in the future
            if (departureDate.Date <= DateTime.Today)
            {
                TempData["Error"] = "Departure date must be at least tomorrow.";
                PopulateErrorViewBag(tour, customerName, customerPhone, customerEmail, departureDate, adultCount, childCount, notes);
                return View();
            }

            // Validate traveler count
            if (adultCount < 1)
            {
                TempData["Error"] = "There must be at least 1 adult for booking.";
                PopulateErrorViewBag(tour, customerName, customerPhone, customerEmail, departureDate, adultCount, childCount, notes);
                return View();
            }

            // Check for double submit (identical booking in last 30 seconds)
            var duplicate = _db.Bookings
                .Where(b => b.UserID == userId 
                         && b.TourID == tourId 
                         && b.DepartureDate.Date == departureDate.Date 
                         && b.Status != "Cancelled")
                .OrderByDescending(b => b.CreatedDate)
                .FirstOrDefault();

            if (duplicate != null && (DateTime.Now - duplicate.CreatedDate).TotalSeconds < 30)
            {
                TempData["Error"] = "You have already submitted an identical booking request recently. Please wait a moment.";
                PopulateErrorViewBag(tour, customerName, customerPhone, customerEmail, departureDate, adultCount, childCount, notes);
                return View();
            }

            // Kiểm tra số chỗ còn lại
            int bookedSlots = _db.Bookings
                .Where(b => b.TourID == tourId
                         && b.DepartureDate.Date == departureDate.Date
                         && b.Status != "Cancelled")
                .Sum(b => (int?)(b.AdultCount + b.ChildCount)) ?? 0;

            int totalRequested = adultCount + childCount;
            int maxSlots = tour.MaxGroupSize ?? int.MaxValue;

            if (bookedSlots + totalRequested > maxSlots)
            {
                TempData["Error"] = $"Not enough slots! Only {Math.Max(0, maxSlots - bookedSlots)} slots left on {departureDate:MM/dd/yyyy}.";
                PopulateErrorViewBag(tour, customerName, customerPhone, customerEmail, departureDate, adultCount, childCount, notes);
                return View();
            }

            // Tính tổng tiền
            decimal totalPrice = (adultCount * tour.PricePerPerson) + (childCount * tour.PricePerChild);

            var booking = new Booking
            {
                TourID        = tourId,
                UserID        = (int)userId,
                CustomerName  = customerName,
                CustomerPhone = customerPhone,
                CustomerEmail = customerEmail,
                DepartureDate = departureDate,
                AdultCount    = adultCount,
                ChildCount    = childCount,
                TotalPrice    = totalPrice,
                Notes         = notes,
                Status        = "Pending",
                PaymentStatus = "Unpaid",
                CreatedDate   = DateTime.Now
            };

            _db.Bookings.Add(booking);
            _db.SaveChanges();

            TempData["Success"] = "Tour booked successfully! Please proceed to payment to confirm your booking.";
            return RedirectToAction("MyBookings");
        }

        private void PopulateErrorViewBag(Tour tour, string customerName, string customerPhone, string customerEmail, DateTime departureDate, int adultCount, int childCount, string? notes)
        {
            int bookedSlots = _db.Bookings
                .Where(b => b.TourID == tour.TourID 
                         && b.DepartureDate.Date == departureDate.Date 
                         && b.Status != "Cancelled")
                .Sum(b => (int?)(b.AdultCount + b.ChildCount)) ?? 0;

            ViewBag.Tour = tour;
            ViewBag.AvailableSlots = Math.Max(0, (tour.MaxGroupSize ?? 0) - bookedSlots);
            ViewBag.CustomerName = customerName;
            ViewBag.CustomerPhone = customerPhone;
            ViewBag.CustomerEmail = customerEmail;
            ViewBag.DefaultDate = departureDate.ToString("yyyy-MM-dd");
            ViewBag.AdultCount = adultCount;
            ViewBag.ChildCount = childCount;
            ViewBag.Notes = notes;
        }

        // ================= ĐẶT TOUR LINH HOẠT =================

        // GET: /Booking/RequestCustom
        public IActionResult RequestCustom()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = _db.Users.Find(userId);
            ViewBag.CustomerName = user?.FullName ?? user?.Username ?? "";
            return View();
        }

        // POST: /Booking/RequestCustom
        [HttpPost]
        public IActionResult RequestCustom(string destinations, DateTime departureDate,
            int durationDays, int adultCount, int childCount,
            string? hotelStandard, string? transportType, string? note)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            var request = new CustomTourRequest
            {
                UserID        = (int)userId,
                Destinations  = destinations,
                DepartureDate = departureDate,
                DurationDays  = durationDays,
                AdultCount    = adultCount,
                ChildCount    = childCount,
                HotelStandard = hotelStandard,
                TransportType = transportType,
                Note          = note,
                Status        = "Pending",
                CreatedDate   = DateTime.Now
            };

            _db.CustomTourRequests.Add(request);
            _db.SaveChanges();

            TempData["Success"] = "Custom request submitted successfully! Admin will send you a quote shortly.";
            return RedirectToAction("MyBookings");
        }

        // ================= LỊCH SỬ ĐẶT TOUR =================

        // GET: /Booking/MyBookings
        public IActionResult MyBookings()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            var bookings = _db.Bookings
                .Include(b => b.Tour)
                .Where(b => b.UserID == (int)userId)
                .OrderByDescending(b => b.CreatedDate)
                .ToList();

            var customRequests = _db.CustomTourRequests
                .Where(r => r.UserID == (int)userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            var hotelBookings = _db.HotelBookings
                .Include(hb => hb.Hotel)
                .Where(hb => hb.UserID == (int)userId)
                .OrderByDescending(hb => hb.CreatedDate)
                .ToList();

            ViewBag.CustomRequests = customRequests;
            ViewBag.HotelBookings = hotelBookings;
            return View(bookings);
        }

        // POST: /Booking/Cancel
        [HttpPost]
        public IActionResult Cancel(int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            var booking = _db.Bookings.FirstOrDefault(b => b.BookingID == bookingId && b.UserID == (int)userId);
            if (booking == null) return NotFound();

            if (booking.Status != "Pending")
            {
                TempData["Error"] = "Only bookings in 'Pending' status can be cancelled.";
                return RedirectToAction("MyBookings");
            }

            booking.Status = "Cancelled";
            _db.SaveChanges();

            TempData["Success"] = "Booking has been successfully cancelled.";
            return RedirectToAction("MyBookings");
        }

        // POST: /Booking/AcceptCustom
        [HttpPost]
        public IActionResult AcceptCustom(int requestId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            var request = _db.CustomTourRequests.FirstOrDefault(r => r.RequestID == requestId && r.UserID == (int)userId);
            if (request == null) return NotFound();

            if (request.Status != "Quoted")
            {
                TempData["Error"] = "Unable to accept this request.";
                return RedirectToAction("MyBookings");
            }

            request.Status = "Accepted";
            _db.SaveChanges();

            TempData["Success"] = "You have accepted the quote! We will contact you soon for contract signing and payment details.";
            return RedirectToAction("MyBookings");
        }

        // POST: /Booking/RejectCustom
        [HttpPost]
        public IActionResult RejectCustom(int requestId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            var request = _db.CustomTourRequests.FirstOrDefault(r => r.RequestID == requestId && r.UserID == (int)userId);
            if (request == null) return NotFound();

            if (request.Status != "Quoted" && request.Status != "Pending")
            {
                TempData["Error"] = "Unable to cancel this request.";
                return RedirectToAction("MyBookings");
            }

            request.Status = "Rejected";
            _db.SaveChanges();

            TempData["Success"] = "Custom tour request has been successfully declined.";
            return RedirectToAction("MyBookings");
        }

        // ================= THANH TOÁN HÓA ĐƠN =================

        // GET: /Booking/Payment?bookingType=Tour&bookingId=5
        public IActionResult Payment(string bookingType, int bookingId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            if (bookingType == "Tour")
            {
                var booking = _db.Bookings.Include(b => b.Tour).FirstOrDefault(b => b.BookingID == bookingId && b.UserID == (int)userId);
                if (booking == null) return NotFound();
                if (booking.Status != "Confirmed")
                {
                    TempData["Error"] = "Only confirmed bookings can be paid.";
                    return RedirectToAction("MyBookings");
                }
                ViewBag.Booking = booking;
                ViewBag.ItemName = booking.Tour?.Title ?? "Tour Booking";
                ViewBag.TotalPrice = booking.TotalPrice;
            }
            else if (bookingType == "Hotel")
            {
                var booking = _db.HotelBookings.Include(b => b.Hotel).FirstOrDefault(b => b.BookingID == bookingId && b.UserID == (int)userId);
                if (booking == null) return NotFound();
                if (booking.Status != "Confirmed")
                {
                    TempData["Error"] = "Only confirmed bookings can be paid.";
                    return RedirectToAction("MyBookings");
                }
                ViewBag.Booking = booking;
                ViewBag.ItemName = booking.Hotel?.Name ?? "Hotel Booking";
                ViewBag.TotalPrice = booking.TotalPrice;
            }
            else
            {
                return BadRequest();
            }

            ViewBag.BookingType = bookingType;
            ViewBag.BookingId = bookingId;
            return View();
        }

        // POST: /Booking/ProcessPayment
        [HttpPost]
        public IActionResult ProcessPayment(string bookingType, int bookingId, string paymentMethod)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Account");

            decimal price = 0;
            if (bookingType == "Tour")
            {
                var booking = _db.Bookings.FirstOrDefault(b => b.BookingID == bookingId && b.UserID == (int)userId);
                if (booking == null) return NotFound();
                price = booking.TotalPrice;
                if (paymentMethod == "COD" || paymentMethod == "BankTransfer")
                {
                    booking.PaymentStatus = "Paid";
                    _db.SaveChanges();
                    TempData["Success"] = $"Payment successful via {paymentMethod}!";
                    return RedirectToAction("MyBookings");
                }
            }
            else if (bookingType == "Hotel")
            {
                var booking = _db.HotelBookings.FirstOrDefault(b => b.BookingID == bookingId && b.UserID == (int)userId);
                if (booking == null) return NotFound();
                price = booking.TotalPrice;
                if (paymentMethod == "COD" || paymentMethod == "BankTransfer")
                {
                    booking.PaymentStatus = "Paid";
                    _db.SaveChanges();
                    TempData["Success"] = $"Payment successful via {paymentMethod}!";
                    return RedirectToAction("MyBookings");
                }
            }

            if (paymentMethod == "VNPay")
            {
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                string returnUrl = $"{Request.Scheme}://{Request.Host}/Booking/PaymentCallback";
                string payUrl = _vnpay.CreatePaymentUrl(HttpContext, bookingType, bookingId, price, ipAddress, returnUrl);
                return Redirect(payUrl);
            }

            return RedirectToAction("MyBookings");
        }

        // GET: /Booking/PaymentCallback
        public IActionResult PaymentCallback()
        {
            if (_vnpay.ValidateSignature(Request.Query, out string bookingType, out int bookingId, out string responseCode))
            {
                if (responseCode == "00")
                {
                    if (bookingType == "Tour")
                    {
                        var booking = _db.Bookings.Find(bookingId);
                        if (booking != null)
                        {
                            booking.PaymentStatus = "Paid";
                            _db.SaveChanges();
                        }
                    }
                    else if (bookingType == "Hotel")
                    {
                        var booking = _db.HotelBookings.Find(bookingId);
                        if (booking != null)
                        {
                            booking.PaymentStatus = "Paid";
                            _db.SaveChanges();
                        }
                    }
                    TempData["Success"] = "VNPay payment completed successfully!";
                }
                else
                {
                    TempData["Error"] = $"VNPay payment was not successful. Response code: {responseCode}";
                }
            }
            else
            {
                TempData["Error"] = "Invalid signature or transaction error.";
            }

            return RedirectToAction("MyBookings");
        }
    }
}
