using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.ProductDTO
{
    public class UpdateProductDto
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
    }
}
