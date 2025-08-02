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
            var item = cart.FirstOrDefault(c => c.ClothingItemId == id);

            if (item != null)
            {
                if (item.Quantity > 1)
                    item.Quantity--;
                else
                    cart.Remove(item);

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

        // Checkout Page
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        // Order Confirmation Page (after placing order)
        public IActionResult CheckoutConfirmation()
        {
            return View();
        }

        // Buy Now / Add to Cart
        [HttpPost]
        public IActionResult BuyNow(int id)
        {
            var item = _context.ClothingItems.FirstOrDefault(i => i.Id == id);
            if (item == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(ci => ci.ClothingItemId == id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ClothingItemId = item.Id,
                    Name = item.Name,
                    Price = item.Price,
                    Quantity = 1
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Checkout");
        }

        // Place Order and Save to DB
        [HttpPost]
        public IActionResult PlaceOrder()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (!cart.Any() || userId == 0)
            {
                TempData["Error"] = "Your cart is empty or user not found.";
                return RedirectToAction("Index");
            }

            // Create Order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(i => i.Price * i.Quantity),
                Items = new List<OrderItem>()
            };

            foreach (var item in cart)
            {
                order.Items.Add(new OrderItem
                {
                    ClothingItemId = item.ClothingItemId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");
            TempData["Success"] = "Your order has been placed successfully!";
            return RedirectToAction("CheckoutConfirmation");
        }
    }
}
