using Microsoft.AspNetCore.Mvc;
using RubenClothingStore.Data;
using RubenClothingStore.Models;
using RubenClothingStore.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RubenClothingStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helpers 
        private List<CartItem> GetCart()
            => HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

        private void SaveCart(List<CartItem> cart)
            => HttpContext.Session.SetObjectAsJson("Cart", cart);

        // Cart Index 
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // Quantity Updates
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int id, string? size, int quantity)
        {
            if (quantity < 1) quantity = 1;

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ClothingItemId == id && c.Size == size);
            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
                TempData["CartMessage"] = "Cart updated.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Increment(int id, string? size)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ClothingItemId == id && c.Size == size);
            if (item != null)
            {
                item.Quantity++;
                SaveCart(cart);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Decrement(int id, string? size)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ClothingItemId == id && c.Size == size);
            if (item != null)
            {
                item.Quantity = Math.Max(1, item.Quantity - 1);
                SaveCart(cart);
            }
            return RedirectToAction(nameof(Index));
        }

        // Remove / Clear
        public IActionResult Remove(int id, string? size)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ClothingItemId == id && c.Size == size);

            if (item != null)
            {
                if (item.Quantity > 1)
                    item.Quantity--;
                else
                    cart.Remove(item);

                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        // Clear the entire cart
        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction(nameof(Index));
        }

        // Checkout 
        public IActionResult Checkout()
        {
            var cart = GetCart();
            return View(cart);
        }

        public IActionResult CheckoutConfirmation()
        {
            return View();
        }

        // Add to Cart / Buy Now 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuyNow(int id, string? size)
        {
            var product = _context.ClothingItems.FirstOrDefault(i => i.Id == id);
            if (product == null) return NotFound();

            var cart = GetCart();

            // Merge by (id, size)
            var existingItem = cart.FirstOrDefault(ci => ci.ClothingItemId == id && ci.Size == size);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ClothingItemId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    Size = size
                });
            }

            SaveCart(cart);

            // Go straight to checkout
            return RedirectToAction(nameof(Checkout));
        }

        // Place Order 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder()
        {
            var cart = GetCart();
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (!cart.Any() || userId == 0)
            {
                TempData["Error"] = "Your cart is empty or user not found.";
                return RedirectToAction(nameof(Index));
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(i => i.Price * i.Quantity),
                Items = new List<OrderItem>()
            };

            foreach (var i in cart)
            {
                order.Items.Add(new OrderItem
                {
                    ClothingItemId = i.ClothingItemId,
                    Quantity = i.Quantity,
                    Price = i.Price,
                   
                });
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");
            TempData["Success"] = "Your order has been placed successfully!";
            return RedirectToAction(nameof(CheckoutConfirmation));
        }
    }
}
