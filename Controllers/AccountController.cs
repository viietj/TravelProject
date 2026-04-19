using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TravelProject.Models;
using System.Security.Cryptography;
using System.Text;

namespace TravelProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly TravelDbContext _db;

        public AccountController(TravelDbContext db)
        {
            _db = db;
        }

        // GET: /Account/Login
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var hashedPassword = HashPassword(password);
            var user = _db.Users
                .FirstOrDefault(u => u.Username == username 
                                  && u.Password == hashedPassword
                                  && u.Status == true);

            if (user == null)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            // Lưu session
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Username", user.Username ?? "");
            HttpContext.Session.SetString("Role", user.Role ?? "User");

            return user.Role == "Admin" 
                ? RedirectToAction("Index", "Admin") 
                : RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(User model)
        {
            // Kiểm tra username đã tồn tại chưa
            if (_db.Users.Any(u => u.Username == model.Username))
            {
                ViewBag.Error = "Username already exists.";
                return View();
            }

            // Kiểm tra email đã tồn tại chưa
            if (_db.Users.Any(u => u.Email == model.Email))
            {
                ViewBag.Error = "Email already exists.";
                return View();
            }

            model.Password = HashPassword(model.Password);
            model.Role = "User";
            model.Status = true;
            model.CreatedDate = DateTime.Now;

            _db.Users.Add(model);
            _db.SaveChanges();

            return RedirectToAction("Login");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Hash password SHA256
        private string HashPassword(string? password)
        {
            if (string.IsNullOrEmpty(password)) return "";
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
    
