using Microsoft.AspNetCore.Mvc;
using TravelProject.Models;

namespace TravelProject.Controllers
{
    public class TourAdminController : Controller
    {
        private readonly TravelDbContext _db;
        public TourAdminController(TravelDbContext db) => _db = db;

        private IActionResult? AdminGuard()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");
            return null;
        }

        
        public IActionResult Index()
        {
            var guard = AdminGuard(); if (guard != null) return guard;
            var tours = _db.Tours.Where(t => t.IsActive).ToList();
            return View(tours);
        }

        public IActionResult Create()
        {
            var guard = AdminGuard(); if (guard != null) return guard;
            ViewBag.Destinations = _db.Destinations.Where(d => d.IsActive).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Tour model)
        {
            var guard = AdminGuard(); if (guard != null) return guard;
            model.IsActive = true;
            model.CreatedBy = HttpContext.Session.GetInt32("UserID");
            model.CreatedDate = DateTime.Now;
            _db.Tours.Add(model);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var guard = AdminGuard(); if (guard != null) return guard;
            var tour = _db.Tours.Find(id);
            if (tour == null) return NotFound();
            ViewBag.Destinations = _db.Destinations.Where(d => d.IsActive).ToList();
            return View(tour);
        }

        [HttpPost]
        public IActionResult Edit(Tour model)
        {
            var guard = AdminGuard(); if (guard != null) return guard;

            var existing = _db.Tours.Find(model.TourID);
            if (existing == null) return NotFound();

            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.Region = model.Region;
            existing.TourType = model.TourType;
            existing.DurationDays = model.DurationDays;
            existing.DurationNights = model.DurationNights;
            existing.MaxGroupSize = model.MaxGroupSize;
            existing.PricePerPerson = model.PricePerPerson;
            existing.PricePerChild = model.PricePerChild;
            existing.OriginalPrice = model.OriginalPrice;
            existing.ImageUrl = model.ImageUrl;
            existing.DepartureCity = model.DepartureCity;
            existing.IsFeatured = model.IsFeatured;
            existing.MainDestinationID = model.MainDestinationID;

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var guard = AdminGuard(); if (guard != null) return guard;
            var tour = _db.Tours.Find(id);
            if (tour != null)
            {
                tour.IsActive = false;
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}