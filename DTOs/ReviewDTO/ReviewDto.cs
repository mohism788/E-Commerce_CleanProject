namespace E_Commerce.DTOs.ReviewDTO
{
    public class ReviewDto
    {
        public int ProductId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; } // e.g., 1 to 5
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Username { get; set; }
    }
}
