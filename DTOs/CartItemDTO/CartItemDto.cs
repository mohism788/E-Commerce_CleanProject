using E_Commerce.DTOs.ProductDTO;

namespace E_Commerce.DTOs.CartItemDTO
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserName { get; set; }
        public int Quantity { get; set; }
        public ProductDto Product { get; set; }
    }
}
