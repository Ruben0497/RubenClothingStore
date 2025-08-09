using System;
using System.Collections.Generic;
using RubenClothingStore.Models;

namespace RubenClothingStore.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<ClothingItem> Items { get; set; } = Array.Empty<ClothingItem>();

        // Filters / search
        public string? Search { get; set; }
        public string? Size { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public string? SortOrder { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        // For dropdowns
        public IEnumerable<string> Sizes { get; set; } = Array.Empty<string>();
    }
}
