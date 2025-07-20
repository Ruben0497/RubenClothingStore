using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RubenClothingStore.Models
{

    public class ClothingItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; } 
        public string Size { get; set; }
        public byte[]? ImageData { get; set; } // Store image as byte array in DB
        public string? ImageMimeType { get; set; } // To support image display

    }
}
