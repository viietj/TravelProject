using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelProject.Models;

namespace TravelProject.Controllers
{
    public class DestinationController : Controller
    {
        private readonly TravelDbContext _db;
        public DestinationController(TravelDbContext db)
        {
            _db = db;
        }

        
        private IActionResult? AdminGuard()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");
            return null;
        }

        
        public IActionResult Index()
        {
            var destinations = _db.Destinations
                .Where(d => d.IsActive == true)
                .ToList();
            return View(destinations);
        }

        
        public IActionResult Create()
        {
            var guard = AdminGuard(); if (guard != null) return guard;
            return View();
        }

        
        [HttpPost]
        public IActionResult Create(Destination model)
        {
            var guard = AdminGuard(); if (guard != null) return guard;

            model.CreatedBy = HttpContext.Session.GetInt32("UserID");
            model.CreatedDate = DateTime.Now;
            model.IsActive = true;
            model.ViewCount = 0;

            _db.Destinations.Add(model);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        
        public IActionResult Edit(int id)
        {
            var guard = AdminGuard(); if (guard != null) return guard;

            var destination = _db.Destinations.Find(id);
            if (destination == null) return NotFound();
            return View(destination);
        }

        
        [HttpPost]
        public IActionResult Edit(Destination model)
        {
            var guard = AdminGuard(); if (guard != null) return guard;

            var existing = _db.Destinations.Find(model.DestinationID);
            if (existing == null) return NotFound();

            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.Address = model.Address;
            existing.Region = model.Region;
            existing.Category = model.Category;
            existing.ImageUrl = model.ImageUrl;
            existing.Latitude = model.Latitude;
            existing.Longitude = model.Longitude;
            existing.City = model.City;

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        
        public IActionResult Delete(int id)
        {
            var guard = AdminGuard(); if (guard != null) return guard;

            var destination = _db.Destinations.Find(id);
            if (destination == null) return NotFound();
            return View(destination);
        }

        
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var guard = AdminGuard(); if (guard != null) return guard;

            var destination = _db.Destinations.Find(id);
            if (destination != null)
            {
                destination.IsActive = false;
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        
        public IActionResult Details(int id)
        {
            var destination = _db.Destinations.Find(id);
            if (destination == null) return NotFound();

            destination.ViewCount++;
            _db.SaveChanges();

            return View(destination);
        }
    }
}