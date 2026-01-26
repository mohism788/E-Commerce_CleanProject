namespace E_Commerce.DTOs.OrderItemDTO
{
    public class CreateOrderItemDto
    {
        public int? OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
