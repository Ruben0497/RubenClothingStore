using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubenClothingStore.Data;
using RubenClothingStore.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RubenClothingStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                // Create default user profile
                var profile = new UserProfile
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    PhoneNumber = null,
                    Address = null,
                    DateOfBirth = null
                };
                _context.UserProfiles.Add(profile);
                _context.SaveChanges();

                // Save session
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin ? "true" : "false");

                
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin ? "true" : "false");

                TempData["Message"] = $"Welcome back, {user.FullName}!";

                if (user.IsAdmin)
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Email is required.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "Email not found.";
                return View();
            }

            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
            await _context.SaveChangesAsync();

            return RedirectToAction("ResetPassword", new { token = user.ResetToken });
        }

        // GET: Account/ResetPassword
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Invalid request.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            ViewBag.Token = token;
            return View();
        }

        // POST: Account/ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
            {
                ViewBag.Error = "Invalid input.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
            {
                ViewBag.Error = "Invalid or expired token.";
                return View();
            }

            user.Password = newPassword;
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            ViewBag.Message = "Password has been reset successfully.";
            return RedirectToAction("Login");
        }
    }
}
