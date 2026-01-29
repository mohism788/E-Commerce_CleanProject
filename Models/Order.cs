namespace E_Commerce.Models
{
    public class Order : BaseEntity
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        //status: Pending, Shipped, Delivered, Cancelled
        public string Status { get; set; }

        //navigation property
        public User User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
