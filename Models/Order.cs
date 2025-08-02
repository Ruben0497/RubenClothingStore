﻿
namespace RubenClothingStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
