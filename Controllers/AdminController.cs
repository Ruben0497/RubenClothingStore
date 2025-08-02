using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubenClothingStore.Data;

namespace RubenClothingStore.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            // Restrict access to Admin only
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            return View();
        }

        public IActionResult Orders()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .ThenInclude(i => i.ClothingItem)
                .ToList();

            return View(orders);
        }


    }
}
