using Microsoft.AspNetCore.Mvc;
using TravelProject.Models;

namespace TravelProject.Controllers
{
    public class UserController : Controller
    {
        private readonly TravelDbContext _db;
        public UserController(TravelDbContext db) => _db = db;

        private IActionResult? AdminGuard()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");
            return null;
        }

        
        public IActionResult Index()
        {
            var guard = AdminGuard(); if (guard != null) return guard;
            var users = _db.Users.ToList();
            return View(users);
        }

        
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var guard = AdminGuard(); if (guard != null) return guard;
            var user = _db.Users.Find(id);
            if (user != null)
            {
                user.Status = !user.Status;
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        
        [HttpPost]
        public IActionResult SetRole(int id, string role)
        {
            var guard = AdminGuard(); if (guard != null) return guard;
            var user = _db.Users.Find(id);
            if (user != null)
            {
                user.Role = role;
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}