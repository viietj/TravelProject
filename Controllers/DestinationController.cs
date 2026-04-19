using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelProject.Models;

namespace TravelProject.Controllers
{
    public class DestinationController: Controller
    {
        private readonly TravelDbContext _db;
        public DestinationController(TravelDbContext db)
        {
            _db = db;
        }

        // GET: /Destination
        public IActionResult Index()
        {
            var destinations = _db.Destinations
                .Where(d => d.IsActive == true)
                .ToList();
            return View(destinations);
        }

        // GET: /Destination/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Destination/Create
        [HttpPost]
        public IActionResult Create(Destination model)
        {
            model.CreatedBy = HttpContext.Session.GetInt32("UserID");
            model.CreatedDate = DateTime.Now;
            model.IsActive = true;
            model.ViewCount = 0;

            _db.Destinations.Add(model);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: /Destination/Edit/5
        public IActionResult Edit(int id)
        {
            var destination = _db.Destinations.Find(id);
            if (destination == null) return NotFound();
            return View(destination);
        }

        // POST: /Destination/Edit/5
        [HttpPost]
        public IActionResult Edit(Destination model)
        {
            _db.Destinations.Update(model);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: /Destination/Delete/5
        public IActionResult Delete(int id)
        {
            var destination = _db.Destinations.Find(id);
            if (destination == null) return NotFound();
            return View(destination);
        }

        // POST: /Destination/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var destination = _db.Destinations.Find(id);
            if (destination != null)
            {
                destination.IsActive = false; // Ẩn thay vì xóa hẳn
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // GET: /Destination/Details/5
        public IActionResult Details(int id)
        {
            var destination = _db.Destinations.Find(id);
            if (destination == null) return NotFound();

            // Tăng lượt xem
            destination.ViewCount++;
            _db.SaveChanges();

            return View(destination);
        }
    }
}