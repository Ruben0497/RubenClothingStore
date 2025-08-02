using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubenClothingStore.Data;

namespace RubenClothingStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.ClothingItems.ToListAsync();
            return View(products);
        }

        // Privacy Policy Page
        public IActionResult Privacy()
        {
            return View();
        }

        // Contact Page (GET)
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        //  Contact Page (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string Name, string Email, string Message)
        {
            // TODO:Save to DB or send email

            TempData["Success"] = "Thank you for reaching out! We will get back to you soon.";
            return RedirectToAction("Contact");
        }
    }
}
