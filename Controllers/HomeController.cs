using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TravelProject.Models;



namespace TravelProject.Controllers;

public class HomeController : Controller
{
    private readonly TravelDbContext _db;

    public HomeController(TravelDbContext db)
    {
        _db = db;
    }

  
  public IActionResult Index()
    {
        var destinations = _db.Destinations
            .Where(d => d.IsActive == true)
            .OrderByDescending(d => d.ViewCount)
            .Take(8) 
            .ToList();
        var tours = _db.Tours
            .OrderByDescending(t => t.TourID)
            .Take(6)
            .ToList();

        ViewBag.Tours = tours;

        var blogs = _db.FoodBlogs
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.CreatedDate)
            .Take(3)
            .ToList();

        ViewBag.Blogs = blogs;

        return View(destinations);
    }

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

    
    public IActionResult DestinationDetail(int id)
    {
        var destination = _db.Destinations.Find(id);
        if (destination == null) return NotFound();

        destination.ViewCount++;
        _db.SaveChanges();

        return View(destination);
    }

[HttpPost]
public IActionResult AddComment(
    int DestinationID,
    string Content,
    int Rating,
    IFormFile? ImageFile)
{
    // Check login
    var userId = HttpContext.Session.GetInt32("UserID");
    if (userId == null)
    {
        return RedirectToAction("Login", "Account");
    }

    // Get login account information from database
    var user = _db.Users.Find(userId);
    string userName = user?.Username ?? "Guest";

    string? imagePath = null;

    
    if (ImageFile != null && ImageFile.Length > 0)
    {
        string folder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot/images/comments");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        string fileName =
            Guid.NewGuid().ToString() +
            Path.GetExtension(ImageFile.FileName);

        string filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            ImageFile.CopyTo(stream);
        }

        imagePath = "/images/comments/" + fileName;
    }

    
    var comment = new Comment
    {
        DestinationID = DestinationID,
        UserName = userName,
        UserID = userId,
        Content = Content,
        Rating = Rating,
        ImageUrl = imagePath,
        CreatedDate = DateTime.Now
    };

    _db.Comments.Add(comment);

    _db.SaveChanges();

    return RedirectToAction(
        "DestinationDetail",
        new { id = DestinationID });
}
    public IActionResult About()
    {
        return View();
    }
    [HttpGet]
public IActionResult GetComments(int destinationId, int page = 1)
{
    int pageSize = 5;
    int userId = HttpContext.Session.GetInt32("UserID") ?? 0;

    var query = _db.Comments.Where(c => c.ParentCommentID == null);

    if (destinationId > 0)
    {
        query = query.Where(c => c.DestinationID == destinationId);
    }

    var rootComments = query
        .OrderByDescending(c => c.CreatedDate)
        .ToList();

    int totalCount = rootComments.Count;
    int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    var paged = rootComments
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    var result = paged.Select(c => new {
        c.CommentID,
        c.DestinationID,
        c.UserName,
        c.Content,
        c.Rating,
        c.ImageUrl,
        c.Likes,
        c.IsAdmin,
        c.CreatedDate,
        IsLiked = userId > 0 && _db.CommentLikes
            .Any(l => l.CommentID == c.CommentID && l.UserID == userId),
        Replies = _db.Comments
            .Where(r => r.ParentCommentID == c.CommentID)
            .OrderBy(r => r.CreatedDate)
            .Select(r => new {
                r.CommentID,
                r.UserName,
                r.Content,
                r.Likes,
                r.IsAdmin,
                r.CreatedDate,
                IsLiked = userId > 0 && _db.CommentLikes
                    .Any(l => l.CommentID == r.CommentID && l.UserID == userId)
            }).ToList()
    });

    return Json(new {
        comments    = result,
        totalPages,
        currentPage = page,
        totalCount
    });
}
[HttpPost]
public IActionResult AddReply(int destinationId, int parentCommentId, string content)
{
    int? userId = HttpContext.Session.GetInt32("UserID");
    if (userId == null)
        return Json(new { success = false, message = "Please log in first." });

    var user = _db.Users.Find(userId);

    var reply = new Comment {
        DestinationID   = destinationId,
        ParentCommentID = parentCommentId,
        UserName        = user?.Username ?? "Guest",
        Content         = content,
        IsAdmin         = HttpContext.Session.GetString("Role") == "Admin",
        CreatedDate     = DateTime.Now
    };

    _db.Comments.Add(reply);
    _db.SaveChanges();

    return Json(new { success = true });
}
[HttpPost]
public IActionResult ToggleLike(int commentId)
{
    int? userId = HttpContext.Session.GetInt32("UserID");
    if (userId == null)
        return Json(new { success = false, message = "Please log in to like this comment." });

    var comment = _db.Comments.Find(commentId);
    if (comment == null)
        return Json(new { success = false });

    var existing = _db.CommentLikes
        .FirstOrDefault(l => l.CommentID == commentId
                          && l.UserID == userId);

    bool isNowLiked;
    if (existing != null)
    {
        // Already liked -> unlike
        _db.CommentLikes.Remove(existing);
        comment.Likes = Math.Max(0, comment.Likes - 1);
        isNowLiked = false;
    }
    else
    {
        // Not yet liked -> add
        _db.CommentLikes.Add(new CommentLike {
            CommentID = commentId,
            UserID    = (int)userId
        });
        comment.Likes++;
        isNowLiked = true;
    }

    _db.SaveChanges();
    return Json(new { success = true, isLiked = isNowLiked, likes = comment.Likes });
}
public IActionResult Blog()
{
    return View();
}
    
}