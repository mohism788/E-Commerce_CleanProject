using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Models
{
    public class User : IdentityUser<Guid>
    {
       
        public DateTime CreatedAt { get; set; }
       


        //navigation property
        public List<Product> Products { get; set; }

        public List<Order> Orders { get; set; }

        public List<Review> Reviews { get; set; }
    }
}
