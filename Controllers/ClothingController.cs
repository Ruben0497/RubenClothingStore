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
using RubenClothingStore.ViewModels;

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
        public async Task<IActionResult> Index(
            string? search,
            string? size,
            int? minPrice,
            int? maxPrice,
            string? sortOrder,
            int page = 1,
            int pageSize = 12)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 12;

            var query = _context.ClothingItems.AsNoTracking().AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x =>
                    x.Name.Contains(s) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(s)));
            }

            // FILTERS
            if (!string.IsNullOrWhiteSpace(size))
                query = query.Where(x => x.Size == size);

            if (minPrice.HasValue)
                query = query.Where(x => x.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(x => x.Price <= maxPrice.Value);

            // SORT
            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(x => x.Price),
                "price_desc" => query.OrderByDescending(x => x.Price),
                "name_desc" => query.OrderByDescending(x => x.Name),
                "newest" => query.OrderByDescending(x => x.Id), 
                _ => query.OrderBy(x => x.Name)          
            };

            // TOTAL for pagination
            var totalItems = await query.CountAsync();

            // PAGINATION
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // DISTINCT SIZES for dropdown
            var sizes = await _context.ClothingItems
                .Where(x => !string.IsNullOrEmpty(x.Size))
                .Select(x => x.Size!)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            var vm = new ProductListViewModel
            {
                Items = items,
                Search = search,
                Size = size,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Sizes = sizes
            };

            return View(vm);
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
