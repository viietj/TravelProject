using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelProject.Models;

namespace TravelProject.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly TravelDbContext _db;
        public FavoriteController(TravelDbContext db) => _db = db;

        private int? GetUserID() => HttpContext.Session.GetInt32("UserID");

        // Danh sách yêu thích của user
        public IActionResult Index()
        {
            var userID = GetUserID();
            if (userID == null) return RedirectToAction("Login", "Account");

            var favorites = _db.Favorites
                .Include(f => f.Destination)
                .Where(f => f.UserID == userID && f.Destination!.IsActive)
                .OrderByDescending(f => f.CreatedDate)
                .ToList();

            return View(favorites);
        }

        
        [HttpPost]
        public IActionResult Toggle(int destinationId, string? returnUrl)
        {
            var userID = GetUserID();
            if (userID == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập." });

            var existing = _db.Favorites
                .FirstOrDefault(f => f.UserID == userID && f.DestinationID == destinationId);

            bool isFavorited;
            if (existing != null)
            {
                _db.Favorites.Remove(existing);
                isFavorited = false;
            }
            else
            {
                _db.Favorites.Add(new Favorite
                {
                    UserID        = userID.Value,
                    DestinationID = destinationId
                });
                isFavorited = true;
            }
            _db.SaveChanges();

            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, isFavorited });

            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("DestinationDetail", "Home", new { id = destinationId });
        }

       
        public IActionResult Check(int destinationId)
        {
            var userID = GetUserID();
            if (userID == null) return Json(new { isFavorited = false });

            var isFav = _db.Favorites.Any(f => f.UserID == userID && f.DestinationID == destinationId);
            return Json(new { isFavorited = isFav });
        }
    }
}
