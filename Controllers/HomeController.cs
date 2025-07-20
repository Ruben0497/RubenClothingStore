using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            var clothes = _context.ClothingItems.ToList();
            return View(clothes);
        }
    }
}
