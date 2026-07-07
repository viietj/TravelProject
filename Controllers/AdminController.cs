using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelProject.Models;
using ClosedXML.Excel;
using System.IO;

namespace TravelProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly TravelDbContext _db;
        public AdminController(TravelDbContext db) => _db = db;

        // Helper: check admin role
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.TotalDestinations  = _db.Destinations.Count(d => d.IsActive == true);
            ViewBag.TotalUsers         = _db.Users.Count(u => u.Status == true);
            ViewBag.TotalTours         = _db.Tours.Count(t => t.IsActive);
            ViewBag.TotalBookings      = _db.Bookings.Count(b => b.Status == "Pending");
            ViewBag.TotalCustomReqs    = _db.CustomTourRequests.Count(r => r.Status == "Pending");
            ViewBag.RecentDestinations = _db.Destinations
                .Where(d => d.IsActive == true)
                .OrderByDescending(d => d.CreatedDate)
                .Take(5)
                .ToList();

            return View();
        }

        // ===================== MANAGE STANDARD BOOKINGS =====================

        // GET: /Admin/ManageBookings
        public IActionResult ManageBookings(string? status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var query = _db.Bookings
                .Include(b => b.Tour)
                .Include(b => b.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(b => b.Status == status);

            ViewBag.StatusFilter = status;
            ViewBag.PendingCount   = _db.Bookings.Count(b => b.Status == "Pending");
            ViewBag.ConfirmedCount = _db.Bookings.Count(b => b.Status == "Confirmed");
            ViewBag.CancelledCount = _db.Bookings.Count(b => b.Status == "Cancelled");

            return View(query.OrderByDescending(b => b.CreatedDate).ToList());
        }

        // POST: /Admin/ConfirmBooking
        [HttpPost]
        public IActionResult ConfirmBooking(int bookingId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var booking = _db.Bookings.Find(bookingId);
            if (booking == null) return NotFound();

            booking.Status = "Confirmed";
            _db.SaveChanges();

            TempData["Success"] = $"Booking #B{bookingId} has been confirmed successfully.";
            return RedirectToAction("ManageBookings");
        }

        // POST: /Admin/CancelBooking
        [HttpPost]
        public IActionResult CancelBooking(int bookingId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var booking = _db.Bookings.Find(bookingId);
            if (booking == null) return NotFound();

            booking.Status = "Cancelled";
            _db.SaveChanges();

            TempData["Success"] = $"Booking #B{bookingId} has been cancelled.";
            return RedirectToAction("ManageBookings");
        }

        // POST: /Admin/ConfirmPayment
        [HttpPost]
        public IActionResult ConfirmPayment(int bookingId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var booking = _db.Bookings.Find(bookingId);
            if (booking == null) return NotFound();

            booking.PaymentStatus = "Paid";
            _db.SaveChanges();

            TempData["Success"] = $"Payment for Booking #B{bookingId} has been confirmed successfully.";
            return RedirectToAction("ManageBookings");
        }

        // POST: /Admin/CompleteBooking
        [HttpPost]
        public IActionResult CompleteBooking(int bookingId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var booking = _db.Bookings.Find(bookingId);
            if (booking == null) return NotFound();

            booking.Status = "Completed";
            _db.SaveChanges();

            TempData["Success"] = $"Booking #B{bookingId} has been marked as Completed.";
            return RedirectToAction("ManageBookings");
        }

        // ===================== MANAGE CUSTOM TOUR REQUESTS =====================

        // GET: /Admin/ManageCustomRequests
        public IActionResult ManageCustomRequests(string? status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var query = _db.CustomTourRequests
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            ViewBag.StatusFilter  = status;
            ViewBag.PendingCount  = _db.CustomTourRequests.Count(r => r.Status == "Pending");
            ViewBag.QuotedCount   = _db.CustomTourRequests.Count(r => r.Status == "Quoted");
            ViewBag.AcceptedCount = _db.CustomTourRequests.Count(r => r.Status == "Accepted");

            return View(query.OrderByDescending(r => r.CreatedDate).ToList());
        }

        // POST: /Admin/SendQuote
        [HttpPost]
        public IActionResult SendQuote(int requestId, decimal quotedPrice)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var request = _db.CustomTourRequests.Find(requestId);
            if (request == null) return NotFound();

            request.QuotedPrice = quotedPrice;
            request.Status      = "Quoted";
            _db.SaveChanges();

            TempData["Success"] = $"Quote of {quotedPrice:N0} VND sent to customer for Request #R{requestId}.";
            return RedirectToAction("ManageCustomRequests");
        }

        // POST: /Admin/RejectCustomRequest
        [HttpPost]
        public IActionResult RejectCustomRequest(int requestId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var request = _db.CustomTourRequests.Find(requestId);
            if (request == null) return NotFound();

            request.Status = "Rejected";
            _db.SaveChanges();

            TempData["Success"] = $"Request #R{requestId} has been rejected.";
            return RedirectToAction("ManageCustomRequests");
        }

        

        
        public IActionResult ManageHotelBookings(string? status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var query = _db.HotelBookings
                .Include(b => b.Hotel)
                .Include(b => b.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(b => b.Status == status);

            ViewBag.StatusFilter = status;
            ViewBag.PendingCount = _db.HotelBookings.Count(b => b.Status == "Pending");
            ViewBag.ConfirmedCount = _db.HotelBookings.Count(b => b.Status == "Confirmed");
            ViewBag.CancelledCount = _db.HotelBookings.Count(b => b.Status == "Cancelled");

            return View(query.OrderByDescending(b => b.CreatedDate).ToList());
        }

      
        [HttpPost]
        public IActionResult ConfirmHotelBooking(int bookingId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var booking = _db.HotelBookings.Find(bookingId);
            if (booking == null) return NotFound();

            booking.Status = "Confirmed";
            _db.SaveChanges();

            TempData["Success"] = $"Hotel Reservation #H{bookingId} has been confirmed successfully.";
            return RedirectToAction("ManageHotelBookings");
        }

        
        [HttpPost]
        public IActionResult CancelHotelBooking(int bookingId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var booking = _db.HotelBookings.Find(bookingId);
            if (booking == null) return NotFound();

            booking.Status = "Cancelled";
            _db.SaveChanges();

            TempData["Success"] = $"Hotel Reservation #H{bookingId} has been cancelled.";
            return RedirectToAction("ManageHotelBookings");
        }

        
        [HttpPost]
        public IActionResult ConfirmHotelPayment(int bookingId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var booking = _db.HotelBookings.Find(bookingId);
            if (booking == null) return NotFound();

            booking.PaymentStatus = "Paid";
            _db.SaveChanges();

            TempData["Success"] = $"Payment for Reservation #H{bookingId} has been confirmed successfully.";
            return RedirectToAction("ManageHotelBookings");
        }

        
        [HttpPost]
        public IActionResult CompleteHotelBooking(int bookingId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var booking = _db.HotelBookings.Find(bookingId);
            if (booking == null) return NotFound();

            booking.Status = "Completed";
            _db.SaveChanges();

            TempData["Success"] = $"Hotel Reservation #H{bookingId} has been marked as Completed.";
            return RedirectToAction("ManageHotelBookings");
        }

        
        public IActionResult Revenue(int? year)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            int selectedYear = year ?? DateTime.Today.Year;

            var tourBookings = _db.Bookings
                .Include(b => b.Tour)
                .Where(b => b.PaymentStatus == "Paid" && b.CreatedDate.Year == selectedYear)
                .ToList();

            var hotelBookings = _db.HotelBookings
                .Include(b => b.Hotel)
                .Where(b => b.PaymentStatus == "Paid" && b.CreatedDate.Year == selectedYear)
                .ToList();

            
            var tourMonthly = new decimal[12];
            var hotelMonthly = new decimal[12];
            foreach (var b in tourBookings)
            {
                tourMonthly[b.CreatedDate.Month - 1] += b.TotalPrice;
            }
            foreach (var h in hotelBookings)
            {
                hotelMonthly[h.CreatedDate.Month - 1] += h.TotalPrice;
            }

            
            var tourQuarterly = new decimal[4];
            var hotelQuarterly = new decimal[4];
            foreach (var b in tourBookings)
            {
                int q = (b.CreatedDate.Month - 1) / 3;
                tourQuarterly[q] += b.TotalPrice;
            }
            foreach (var h in hotelBookings)
            {
                int q = (h.CreatedDate.Month - 1) / 3;
                hotelQuarterly[q] += h.TotalPrice;
            }

            
            var topTours = tourBookings
                .GroupBy(b => b.TourID)
                .Select(g => new {
                    TourTitle = g.First().Tour?.Title ?? $"Tour ID {g.Key}",
                    Revenue = g.Sum(b => b.TotalPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToList();

           
            var pendingTour = _db.Bookings.Where(b => b.PaymentStatus != "Paid" && b.Status != "Cancelled" && b.CreatedDate.Year == selectedYear).Sum(b => b.TotalPrice);
            var pendingHotel = _db.HotelBookings.Where(h => h.PaymentStatus != "Paid" && h.Status != "Cancelled" && h.CreatedDate.Year == selectedYear).Sum(h => h.TotalPrice);
            var pendingCount = _db.Bookings.Count(b => b.PaymentStatus != "Paid" && b.Status != "Cancelled" && b.CreatedDate.Year == selectedYear) +
                               _db.HotelBookings.Count(h => h.PaymentStatus != "Paid" && h.Status != "Cancelled" && h.CreatedDate.Year == selectedYear);
            var totalPending = pendingTour + pendingHotel;

            ViewBag.SelectedYear = selectedYear;
            ViewBag.TotalAnnualRevenue = tourBookings.Sum(b => b.TotalPrice) + hotelBookings.Sum(h => h.TotalPrice);
            ViewBag.TourRevenue = tourBookings.Sum(b => b.TotalPrice);
            ViewBag.TourCount = tourBookings.Count;
            ViewBag.HotelRevenue = hotelBookings.Sum(h => h.TotalPrice);
            ViewBag.HotelCount = hotelBookings.Count;
            ViewBag.PendingRevenue = totalPending;
            ViewBag.PendingCount = pendingCount;
            ViewBag.TourMonthly = tourMonthly;
            ViewBag.HotelMonthly = hotelMonthly;
            ViewBag.TourQuarterly = tourQuarterly;
            ViewBag.HotelQuarterly = hotelQuarterly;
            ViewBag.TopTours = topTours;

            
            var bookingYears = _db.Bookings.Select(b => b.CreatedDate.Year)
                .Concat(_db.HotelBookings.Select(h => h.CreatedDate.Year))
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();
            if (!bookingYears.Contains(DateTime.Today.Year))
            {
                bookingYears.Add(DateTime.Today.Year);
            }
            ViewBag.Years = bookingYears.OrderByDescending(y => y).ToList();

            return View();
        }

        
        [HttpGet]
        public IActionResult ExportRevenue(int? year)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            int selectedYear = year ?? DateTime.Today.Year;
            
            var tourBookings = _db.Bookings
                .Include(b => b.Tour)
                .Where(b => b.PaymentStatus == "Paid" && b.CreatedDate.Year == selectedYear)
                .ToList();

            var hotelBookings = _db.HotelBookings
                .Include(h => h.Hotel)
                .Where(h => h.PaymentStatus == "Paid" && h.CreatedDate.Year == selectedYear)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var wsSummary = workbook.Worksheets.Add("Doanh thu chung");
                wsSummary.Cell("A1").Value = $"Bao cao doanh thu nam {selectedYear}";
                wsSummary.Cell("A1").Style.Font.Bold = true;
                wsSummary.Cell("A1").Style.Font.FontSize = 16;

                wsSummary.Cell("A3").Value = "Loai hinh";
                wsSummary.Cell("B3").Value = "So don da thanh toan";
                wsSummary.Cell("C3").Value = "Doanh thu (VND)";
                wsSummary.Range("A3:C3").Style.Font.Bold = true;

                decimal totalTour = tourBookings.Sum(b => b.TotalPrice);
                decimal totalHotel = hotelBookings.Sum(h => h.TotalPrice);

                wsSummary.Cell("A4").Value = "Tour Du lich";
                wsSummary.Cell("B4").Value = tourBookings.Count;
                wsSummary.Cell("C4").Value = totalTour;

                wsSummary.Cell("A5").Value = "Khach san";
                wsSummary.Cell("B5").Value = totalHotel;
                wsSummary.Cell("C5").Value = totalHotel; // Fix: use totalHotel

                wsSummary.Cell("A6").Value = "Tong cong";
                wsSummary.Cell("B6").Value = tourBookings.Count + hotelBookings.Count;
                wsSummary.Cell("C6").Value = totalTour + totalHotel;
                wsSummary.Range("A6:C6").Style.Font.Bold = true;

               
                var wsTour = workbook.Worksheets.Add("Chi tiet Tour");
                wsTour.Cell("A1").Value = "Danh sach don Tour da thanh toan";
                wsTour.Cell("A1").Style.Font.Bold = true;
                 
                string[] tourHeaders = { "Ma Booking", "Ten Tour", "Khach hang", "Ngay khoi hanh", "So nguoi", "Tong tien" };
                for (int i = 0; i < tourHeaders.Length; i++)
                {
                    wsTour.Cell(3, i + 1).Value = tourHeaders[i];
                    wsTour.Cell(3, i + 1).Style.Font.Bold = true;
                }
                 
                for (int row = 0; row < tourBookings.Count; row++)
                {
                    var b = tourBookings[row];
                    wsTour.Cell(row + 4, 1).Value = $"#B{b.BookingID}";
                    wsTour.Cell(row + 4, 2).Value = b.Tour?.Title ?? "—";
                    wsTour.Cell(row + 4, 3).Value = b.CustomerName;
                    wsTour.Cell(row + 4, 4).Value = b.DepartureDate.ToString("dd/MM/yyyy");
                    wsTour.Cell(row + 4, 5).Value = $"{b.AdultCount} nguoi lon, {b.ChildCount} tre em";
                    wsTour.Cell(row + 4, 6).Value = b.TotalPrice;
                }

                
                var wsHotel = workbook.Worksheets.Add("Chi tiet Khach san");
                wsHotel.Cell("A1").Value = "Danh sach don Khach san da thanh toan";
                wsHotel.Cell("A1").Style.Font.Bold = true;
                 
                string[] hotelHeaders = { "Ma Reservation", "Ten Khach san", "Khach hang", "Check-in", "Check-out", "So phong (Loai)", "Tong tien" };
                for (int i = 0; i < hotelHeaders.Length; i++)
                {
                    wsHotel.Cell(3, i + 1).Value = hotelHeaders[i];
                    wsHotel.Cell(3, i + 1).Style.Font.Bold = true;
                }

                for (int row = 0; row < hotelBookings.Count; row++)
                {
                    var hb = hotelBookings[row];
                    wsHotel.Cell(row + 4, 1).Value = $"#H{hb.BookingID}";
                    wsHotel.Cell(row + 4, 2).Value = hb.Hotel?.Name ?? "—";
                    wsHotel.Cell(row + 4, 3).Value = hb.CustomerName;
                    wsHotel.Cell(row + 4, 4).Value = hb.CheckInDate.ToString("dd/MM/yyyy");
                    wsHotel.Cell(row + 4, 5).Value = hb.CheckOutDate.ToString("dd/MM/yyyy");
                    wsHotel.Cell(row + 4, 6).Value = $"{hb.RoomCount} phong ({hb.RoomType})";
                    wsHotel.Cell(row + 4, 7).Value = hb.TotalPrice;
                }

                
                wsSummary.Columns().AdjustToContents();
                wsTour.Columns().AdjustToContents();
                wsHotel.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"BaoCaoDoanhThu_{selectedYear}.xlsx");
                }
            }
        }
        

        
        public IActionResult ManageComments(int? destinationId, string? search)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var query = _db.Comments
                .Include(c => c.Destination)
                .AsQueryable();

            if (destinationId.HasValue)
                query = query.Where(c => c.DestinationID == destinationId);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.UserName.Contains(search) || c.Content.Contains(search));

            ViewBag.DestinationId = destinationId;
            ViewBag.Search = search;
            ViewBag.Destinations = _db.Destinations
                .Where(d => d.IsActive == true)
                .OrderBy(d => d.Name)
                .ToList();

            return View(query.OrderByDescending(c => c.CreatedDate).ToList());
        }

        // POST: /Admin/DeleteComment
        [HttpPost]
        public IActionResult DeleteComment(int commentId, int? destinationId, string? search)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Delete replies first
            var replies = _db.Comments.Where(c => c.ParentCommentID == commentId).ToList();
            _db.Comments.RemoveRange(replies);

            // Delete the comment itself
            var comment = _db.Comments.Find(commentId);
            if (comment != null)
                _db.Comments.Remove(comment);

            _db.SaveChanges();
            TempData["Success"] = $"Comment #{commentId} has been deleted."
;
            return RedirectToAction("ManageComments", new { destinationId, search });
        }
    }
}