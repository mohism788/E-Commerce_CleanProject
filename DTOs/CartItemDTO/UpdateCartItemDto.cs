namespace E_Commerce.DTOs.CartItemDTO
{
    public class UpdateCartItemDto
    {
        public int ProductId { get; set; }
        public Guid userId { get; set; }
        public int Quantity { get; set; }
    }
}
