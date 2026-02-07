using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace E_Commerce.Models
{
    public class Product : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description{ get; set; }
        public decimal Price { get; set; }
        public int Stock{ get; set; }
        public Guid SellerId{ get; set; }

        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }

        //navigation property category, review, orderitems
        public Category Category { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }




    }
}
