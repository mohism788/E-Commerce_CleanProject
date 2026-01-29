namespace E_Commerce.Models
{
    public class Category : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //navigation property
        public ICollection<Product> Products { get; set; }
    }
}
