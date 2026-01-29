using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.ProductDTO
{
    public class ProductQueryParameters
    {// Pagination
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        // Filtering
        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public Guid? SellerId { get; set; }

        // Sorting
        public string? SortBy { get; set; } = "name"; // name, price, createdAt
        public bool SortDescending { get; set; } = false;

        // Search
        public string? SearchTerm { get; set; }

        // In stock only
        public bool InStockOnly { get; set; } = false;
    }
}
