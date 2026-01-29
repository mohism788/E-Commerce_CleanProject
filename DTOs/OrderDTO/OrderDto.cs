using E_Commerce.DTOs.OrderItemDTO;

namespace E_Commerce.DTOs.OrderDTO
{
    public class OrderDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        //status: Pending, Shipped, Delivered, Cancelled
        public string Status { get; set; }
        public IEnumerable<OrderItemWithProductNameDto> OrderItems { get; set; }
    }
}
