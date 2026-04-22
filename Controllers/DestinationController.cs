using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelProject.Models;
using TravelProject.Models.ViewModels;

namespace TravelProject.Controllers
{
    public class DestinationController: Controller
    {
        private readonly TravelDbContext _db;
        public DestinationController(TravelDbContext db)
        {
            _db = db;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
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
            var destination = _db.Destinations.FirstOrDefault(d => d.DestinationID == id && d.IsActive);
            if (destination == null) return NotFound();

            // Tăng lượt xem
            destination.ViewCount++;
            _db.SaveChanges();

            var comments = _db.DestinationComments
                .Where(c => c.DestinationID == id && c.IsActive && c.IsApproved)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            var vm = new DestinationDetailsViewModel
            {
                Destination = destination,
                Comments = comments,
                TotalComments = comments.Count,
                AverageRating = comments.Count > 0 ? comments.Average(c => c.Rating) : 0
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult AddComment(DestinationDetailsViewModel model)
        {
            var destination = _db.Destinations.FirstOrDefault(d => d.DestinationID == model.Destination.DestinationID && d.IsActive);
            if (destination == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model.UserName) ||
                string.IsNullOrWhiteSpace(model.Content) ||
                model.Rating < 1 ||
                model.Rating > 5)
            {
                TempData["CommentError"] = "Please enter your name, review content and choose a rating from 1 to 5 stars.";
                return RedirectToAction("Details", new { id = model.Destination.DestinationID });
            }

            var comment = new DestinationComment
            {
                DestinationID = model.Destination.DestinationID,
                UserName = model.UserName.Trim(),
                Content = model.Content.Trim(),
                Rating = model.Rating,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsApproved = true
            };

            _db.DestinationComments.Add(comment);
            _db.SaveChanges();

            TempData["CommentSuccess"] = "Your review has been submitted successfully.";
            return RedirectToAction("Details", new { id = model.Destination.DestinationID });
        }

        public IActionResult ManageComments()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var comments = _db.DestinationComments
                .Include(c => c.Destination)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            return View(comments);
        }

        public IActionResult ToggleComment(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var comment = _db.DestinationComments.Find(id);
            if (comment == null) return NotFound();

            comment.IsActive = !comment.IsActive;
            _db.SaveChanges();

            return RedirectToAction("ManageComments");
        }

        public IActionResult DeleteComment(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var comment = _db.DestinationComments.Find(id);
            if (comment == null) return NotFound();

            comment.IsActive = false;
            _db.SaveChanges();

            return RedirectToAction("ManageComments");
        }
    }
}
