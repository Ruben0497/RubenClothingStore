using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubenClothingStore.Data;
using RubenClothingStore.Models;
using Newtonsoft.Json;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Order Confirmation (Checkout)
    public IActionResult Confirm()
    {
        int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (userId == 0)
            return RedirectToAction("Login", "Account");

        var cartItems = GetCartFromSession();
        var total = cartItems.Sum(c => c.Price * c.Quantity);

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            TotalAmount = total
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        foreach (var item in cartItems)
        {
            _context.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ClothingItemId = item.ClothingItemId,
                Quantity = item.Quantity,
                Price = item.Price
            });
        }

        _context.SaveChanges();
        ClearCartFromSession();

        return View(order);
    }

    // View Past Orders
    public async Task<IActionResult> History()
    {
        int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (userId == 0)
            return RedirectToAction("Login", "Account");

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        var orderIds = orders.Select(o => o.Id).ToList();

        var orderItems = await _context.OrderItems
            .Where(oi => orderIds.Contains(oi.OrderId))
            .Include(oi => oi.ClothingItem)
            .ToListAsync();

        foreach (var order in orders)
        {
            order.Items = orderItems.Where(i => i.OrderId == order.Id).ToList();
        }

        return View(orders);
    }

    // Get Cart from Session
    private List<CartItem> GetCartFromSession()
    {
        var sessionData = HttpContext.Session.GetString("Cart");
        return string.IsNullOrEmpty(sessionData)
            ? new List<CartItem>()
            : JsonConvert.DeserializeObject<List<CartItem>>(sessionData);
    }

    // Clear Cart
    private void ClearCartFromSession()
    {
        HttpContext.Session.Remove("Cart");
    }
}
