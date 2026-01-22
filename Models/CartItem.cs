namespace E_Commerce.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Guid UserId { get; set; }
        public int Quantity { get; set; }
        //navigation property
        public Product Product { get; set; }
        public User User { get; set; }
    }
}
