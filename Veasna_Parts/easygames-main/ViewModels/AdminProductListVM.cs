// ViewModels/AdminProductListVM.cs
using System;
using System.Collections.Generic;
using EasyGames.Models; // Product

namespace EasyGames.ViewModels
{
    // Admin list VM (used by Areas/Admin/Controllers/ProductsController.Index)
    public class AdminProductListVM
    {
        public IReadOnlyList<Product> Items { get; set; } = Array.Empty<Product>();

        // search + sort
        public string? Q { get; set; } = null;      // search term
        public string Sort { get; set; } = "Id";   // Id | title | category | price | stock
        public string Dir { get; set; } = "asc";  // asc | desc

        // paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int Total { get; set; } = 0;

        // helpers for the view
        public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, PageSize));
        public int StartIndex => Total == 0 ? 0 : ((Page - 1) * PageSize) + 1;
        public int EndIndex => Math.Min(Page * PageSize, Total);
    }
}


