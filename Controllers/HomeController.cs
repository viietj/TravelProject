using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TravelProject.Models;

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
        var destination = _db.Destinations.Find(id);
        if (destination == null) return NotFound();

        destination.ViewCount++;
        _db.SaveChanges();

        return View(destination);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}