namespace E_Commerce.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; } // e.g., 1 to 5
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        //navigation property
        public Product Product { get; set; }
        public User User { get; set; }
    }
}
