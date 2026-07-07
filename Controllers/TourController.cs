using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelProject.Models;

public class TourController : Controller
{
    private readonly TravelDbContext _db;
    public TourController(TravelDbContext db) => _db = db;

    public async Task<IActionResult> Index(
        int? destinationId,
        string? search,
        string? type,
        string? region,
        decimal? maxPrice)
    {
        IQueryable<int> tourIdQuery;

        if (destinationId.HasValue)
        {
            
            var destCity = await _db.Destinations
                .Where(d => d.DestinationID == destinationId)
                .Select(d => d.City)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(destCity))
            {
                tourIdQuery = _db.TourByDestination
                    .Where(t => t.DestinationCity == destCity)
                    .Select(t => t.TourID)
                    .Distinct();

                ViewBag.DestinationName = destCity;
            }
            else
            {
                tourIdQuery = _db.TourByDestination
                    .Where(t => t.DestinationID == destinationId)
                    .Select(t => t.TourID)
                    .Distinct();

                var destName = await _db.Destinations
                    .Where(d => d.DestinationID == destinationId)
                    .Select(d => d.Name)
                    .FirstOrDefaultAsync();
                ViewBag.DestinationName = destName;
            }
        }
        else if (!string.IsNullOrEmpty(search))
        {
            
            tourIdQuery = _db.TourByDestination
                .Where(t => t.DestinationName!.Contains(search)
                         || t.DestinationCity!.Contains(search))
                .Select(t => t.TourID)
                .Distinct();

            ViewBag.DestinationName = search;
        }
        else
        {
           
            tourIdQuery = _db.TourByDestination
                .Select(t => t.TourID)
                .Distinct();
        }

        // Get active tour IDs only
        var activeTourIds = _db.Tours.Where(t => t.IsActive).Select(t => t.TourID);

        var query = _db.TourOverview
            .Where(t => tourIdQuery.Contains(t.TourId) && activeTourIds.Contains(t.TourId));

        if (!string.IsNullOrEmpty(type))
            query = query.Where(t => t.TourType == type);
        if (!string.IsNullOrEmpty(region))
            query = query.Where(t => t.Region == region);
        if (maxPrice.HasValue && maxPrice > 0)
            query = query.Where(t => t.PricePerPerson <= maxPrice);

        ViewBag.DestinationId = destinationId;
        ViewBag.Search   = search;
        ViewBag.Type     = type;
        ViewBag.Region   = region;
        ViewBag.MaxPrice = maxPrice;

        return View(await query.ToListAsync());
    }

    // GET: /Tour/Detail/5
    public async Task<IActionResult> Detail(int id)
    {
        var tour = await _db.Tours.FindAsync(id);
        if (tour == null || !tour.IsActive) return NotFound();

        // Calculate remaining slots for tomorrow (default date)
        var defaultDate = DateTime.Today.AddDays(1);
        int bookedSlots = _db.Bookings
            .Where(b => b.TourID == id && b.DepartureDate.Date == defaultDate.Date && b.Status != "Cancelled")
            .Sum(b => (int?)(b.AdultCount + b.ChildCount)) ?? 0;

        ViewBag.AvailableSlots = Math.Max(0, (tour.MaxGroupSize ?? 0) - bookedSlots);

        // Related tours in the same region
        var related = await _db.Tours
            .Where(t => t.TourID != id && t.IsActive && t.Region == tour.Region)
            .Take(3)
            .ToListAsync();

        ViewBag.RelatedTours = related;

        return View(tour);
    }
}