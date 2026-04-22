using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TravelProject.Models;
using TravelProject.Models.ViewModels;

namespace TravelProject.Controllers;

public class HomeController : Controller
{
    private readonly TravelDbContext _db;

    public HomeController(TravelDbContext db)
    {
        _db = db;
    }

    // Trang chủ
  public IActionResult Index()
    {
    var destinations = _db.Destinations
        .Where(d => d.IsActive == true)
        .OrderByDescending(d => d.ViewCount)
        .Take(8) // Chỉ lấy 8 địa điểm nổi bật
        .ToList();

    return View(destinations);
    }

    // Danh sách địa điểm cho User
    public IActionResult Destinations(string? search, string? region, string? category)
    {
        var destinations = _db.Destinations
            .Where(d => d.IsActive == true)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            destinations = destinations.Where(d => d.Name.Contains(search));

        if (!string.IsNullOrEmpty(region))
            destinations = destinations.Where(d => d.Region == region);

        if (!string.IsNullOrEmpty(category))
            destinations = destinations.Where(d => d.Category == category);

        ViewBag.Search   = search;
        ViewBag.Region   = region;
        ViewBag.Category = category;

        return View(destinations.ToList());
    }

    // Chi tiết địa điểm cho User
    public IActionResult DestinationDetail(int id)
    {
        var destination = _db.Destinations.FirstOrDefault(d => d.DestinationID == id && d.IsActive);
        if (destination == null) return NotFound();

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
    public IActionResult AddDestinationComment(DestinationDetailsViewModel model)
    {
        var destination = _db.Destinations.FirstOrDefault(d => d.DestinationID == model.Destination.DestinationID && d.IsActive);
        if (destination == null) return NotFound();

        if (string.IsNullOrWhiteSpace(model.UserName) ||
            string.IsNullOrWhiteSpace(model.Content) ||
            model.Rating < 1 ||
            model.Rating > 5)
        {
            TempData["CommentError"] = "Please enter your name, review content and choose a rating from 1 to 5 stars.";
            return RedirectToAction("DestinationDetail", new { id = model.Destination.DestinationID });
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
        return RedirectToAction("DestinationDetail", new { id = model.Destination.DestinationID });
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
