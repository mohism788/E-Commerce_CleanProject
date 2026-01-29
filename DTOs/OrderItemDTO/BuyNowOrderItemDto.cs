using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.OrderItemDTO
{
    public class BuyNowOrderItemDto
    {
        [Required(ErrorMessage = "ProductId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive number")]
        public int ProductId { get; set; }


        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
