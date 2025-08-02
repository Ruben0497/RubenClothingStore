namespace RubenClothingStore.Models
{
    public class CartItem
    {
        public int ClothingItemId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; }

    }
}
