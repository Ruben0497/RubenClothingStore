using Microsoft.AspNetCore.Mvc;
using RubenClothingStore.Data;
using RubenClothingStore.Models;
using RubenClothingStore.Helpers;

namespace RubenClothingStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Show Cart Items
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        // Remove or reduce quantity
        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.Item.Id == id);

            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    cart.Remove(item);
                }

                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return RedirectToAction("Index");
        }

        // Clear the entire cart
        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult PlaceOrder()
        {
            // Get cart from session
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            

            // Clear the cart
            HttpContext.Session.Remove("Cart");

            // Redirect to Order Confirmation page or Index
            TempData["Success"] = "Your order has been placed successfully!";
            return RedirectToAction("CheckoutConfirmation");
        }
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart); 
        }

        public IActionResult CheckoutConfirmation()
        {
            return View();
        }

    }
}
