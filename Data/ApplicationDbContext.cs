using E_Commerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions dbContextOptions)
        :base(dbContextOptions)
        {
         
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Category> Categories{ get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
           List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id=1.ToString(),
                    Name="Admin",
                    NormalizedName="ADMIN"
                },
                new IdentityRole
                {
                    Id=2.ToString(),
                    Name="Customer",
                    NormalizedName="CUSTOMER"
                },
                new IdentityRole
                {
                    Id=3.ToString(),
                    Name="Seller",
                    NormalizedName="SELLER"
                }
            };
            builder.Entity<IdentityRole>().HasData(roles);
        }





    }
}
