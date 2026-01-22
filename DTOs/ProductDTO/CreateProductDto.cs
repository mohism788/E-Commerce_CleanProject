namespace E_Commerce.DTOs.ProductDTO
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public Guid SellerId { get; set; }

        public int CategoryId { get; set; }
    }
}
