
using RubenClothingStore.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ClothingItemId { get; set; }
    public ClothingItem? ClothingItem { get; set; }

    public int Quantity { get; set; }
    public decimal Price { get; set; }
    

}
