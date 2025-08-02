using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubenClothingStore.Data;
using RubenClothingStore.Models;
using Microsoft.AspNetCore.Http;
using RubenClothingStore.Helpers;
using System.Collections.Generic;

namespace RubenClothingStore.Controllers
{
    public class ClothingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClothingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clothing
        public async Task<IActionResult> Index()
        {
            return View(await _context.ClothingItems.ToListAsync());
        }

        // GET: Clothing/Welcome
        public async Task<IActionResult> Welcome()
        {
            var items = await _context.ClothingItems.ToListAsync();
            return View(items);
        }

        // GET: Clothing/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var clothingItem = await _context.ClothingItems.FirstOrDefaultAsync(m => m.Id == id);
            if (clothingItem == null)
                return NotFound();

            return View(clothingItem);
        }

        // GET: Clothing/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            return View();
        }

        // POST: Clothing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,Price,Size,Description")] ClothingItem clothingItem,
            IFormFile? imageFile)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        clothingItem.ImageData = memoryStream.ToArray();
                        clothingItem.ImageMimeType = imageFile.ContentType;
                    }
                }

                _context.Add(clothingItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(clothingItem);
        }

        // GET: Clothing/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            if (id == null)
                return NotFound();

            var clothingItem = await _context.ClothingItems.FindAsync(id);
            if (clothingItem == null)
                return NotFound();

            return View(clothingItem);
        }

        // POST: Clothing/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,Price,Size,Description")] ClothingItem clothingItem,
            IFormFile? imageFile)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            if (id != clothingItem.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingItem = await _context.ClothingItems.FindAsync(id);
                    if (existingItem == null)
                        return NotFound();

                    existingItem.Name = clothingItem.Name;
                    existingItem.Price = clothingItem.Price;
                    existingItem.Size = clothingItem.Size;
                    existingItem.Description = clothingItem.Description;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await imageFile.CopyToAsync(memoryStream);
                            existingItem.ImageData = memoryStream.ToArray();
                            existingItem.ImageMimeType = imageFile.ContentType;
                        }
                    }

                    _context.Update(existingItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClothingItemExists(clothingItem.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(clothingItem);
        }

        // GET: Clothing/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            if (id == null)
                return NotFound();

            var clothingItem = await _context.ClothingItems.FirstOrDefaultAsync(m => m.Id == id);
            if (clothingItem == null)
                return NotFound();

            return View(clothingItem);
        }

        // POST: Clothing/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return Unauthorized();

            var clothingItem = await _context.ClothingItems.FindAsync(id);
            if (clothingItem != null)
                _context.ClothingItems.Remove(clothingItem);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClothingItemExists(int id)
        {
            return _context.ClothingItems.Any(e => e.Id == id);
        }

        // POST: Clothing/AddToCart
        [HttpPost]
        public IActionResult AddToCart(int id)
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
                    Size = item.Size,
                    Price = item.Price,
                    Quantity = 1
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }
    }
}
