namespace E_Commerce.DTOs.OrderDTO
{
    public class CreateOrderDto
    {
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        //status: Pending, Shipped, Delivered, Cancelled
        public string Status { get; set; }

    }
}
