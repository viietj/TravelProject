using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelProject.Models;

namespace TravelProject.Controllers
{
    public class FoodBlogController : Controller
    {
        private readonly TravelDbContext _db;

        public FoodBlogController(TravelDbContext db)
        {
            _db = db;
        }

        // GET: /FoodBlog  or  /FoodBlog?region=North&season=Winter
        public IActionResult Index(string? region, string? season)
        {
            var query = _db.FoodBlogs
                .Where(b => b.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(region))
                query = query.Where(b => b.Region == region);

            if (!string.IsNullOrEmpty(season))
                query = query.Where(b => b.Season == season);

            ViewBag.Region = region;
            ViewBag.Season = season;

            return View(query.OrderByDescending(b => b.CreatedDate).ToList());
        }

        // GET: /FoodBlog/Detail/5
        public IActionResult Detail(int id)
        {
            var blog = _db.FoodBlogs
                .Include(b => b.Restaurants)
                    .ThenInclude(r => r.Reviews)
                .FirstOrDefault(b => b.BlogID == id && b.IsActive);

            if (blog == null) return NotFound();
            return View(blog);
        }

        // POST: /FoodBlog/AddReview
        [HttpPost]
        public IActionResult AddReview(int restaurantId, int blogId, int rating, string comment)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _db.Users.Find(userId);

            _db.FoodReviews.Add(new FoodReview
            {
                RestaurantID = restaurantId,
                UserID       = (int)userId,
                UserName     = user?.Username ?? "Guest",
                Rating       = rating,
                Comment      = comment,
                CreatedDate  = DateTime.Now
            });
            _db.SaveChanges();

            return RedirectToAction("Detail", new { id = blogId });
        }

        // ── ADMIN ─────────────────────────────────────────────────────────

        public IActionResult Manage()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Home");

            var blogs = _db.FoodBlogs
                .Include(b => b.Restaurants)
                .OrderByDescending(b => b.CreatedDate)
                .ToList();

            return View(blogs);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(FoodBlog blog, IFormFile? ImageFile)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Home");

            if (ImageFile != null && ImageFile.Length > 0)
                blog.ImageUrl = await SaveImage(ImageFile, "blog");

            blog.CreatedDate = DateTime.Now;
            _db.FoodBlogs.Add(blog);
            _db.SaveChanges();
            return RedirectToAction("Manage");
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Home");

            var blog = _db.FoodBlogs.Include(b => b.Restaurants).FirstOrDefault(b => b.BlogID == id);
            if (blog == null) return NotFound();
            return View(blog);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FoodBlog blog, IFormFile? ImageFile)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Home");

            var existing = _db.FoodBlogs.Find(blog.BlogID);
            if (existing == null) return NotFound();

            existing.Title       = blog.Title;
            existing.Description = blog.Description;
            existing.Content     = blog.Content;
            existing.Region      = blog.Region;
            existing.Season      = blog.Season;
            existing.IsActive    = blog.IsActive;

            if (ImageFile != null && ImageFile.Length > 0)
                existing.ImageUrl = await SaveImage(ImageFile, "blog");

            _db.SaveChanges();
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Home");

            var blog = _db.FoodBlogs.Find(id);
            if (blog != null) { _db.FoodBlogs.Remove(blog); _db.SaveChanges(); }
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public async Task<IActionResult> AddRestaurant(FoodRestaurant restaurant, IFormFile? ImageFile)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Home");

            if (ImageFile != null && ImageFile.Length > 0)
                restaurant.ImageUrl = await SaveImage(ImageFile, "restaurants");

            _db.FoodRestaurants.Add(restaurant);
            _db.SaveChanges();
            return RedirectToAction("Edit", new { id = restaurant.BlogID });
        }

        [HttpPost]
        public IActionResult DeleteRestaurant(int restaurantId, int blogId)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Home");

            var r = _db.FoodRestaurants.Find(restaurantId);
            if (r != null) { _db.FoodRestaurants.Remove(r); _db.SaveChanges(); }
            return RedirectToAction("Edit", new { id = blogId });
        }

        // ── HELPER ────────────────────────────────────────────────────────
        private async Task<string> SaveImage(IFormFile file, string folder)
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", folder);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string path = Path.Combine(dir, fileName);
            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/images/{folder}/{fileName}";
        }
    }
}