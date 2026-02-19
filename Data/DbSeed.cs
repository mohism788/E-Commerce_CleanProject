using E_Commerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Data
{
    public static class DbSeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            // 1. Ensure Categories exist
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Computers & Laptops" },
                    new Category { Name = "Smartphones & Tablets" },
                    new Category { Name = "Audio & Headphones" },
                    new Category { Name = "Gaming & Consoles" },
                    new Category { Name = "Home Appliances" }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            var allCategories = await context.Categories.ToListAsync();

            // 2. Ensure Users exist (Sellers and Customers)
            var sellerEmails = new[] { "seller1@berserk.com", "seller2@berserk.com" };
            var customerEmails = new[] { "customer1@gmail.com", "customer2@gmail.com", "customer3@gmail.com" };

            // Create Sellers
            foreach (var email in sellerEmails)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new User
                    {
                        UserName = email.Split('@')[0],
                        Email = email,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    var result = await userManager.CreateAsync(user, "Password123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Seller");
                    }
                }
            }

            // Create Customers
            foreach (var email in customerEmails)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new User
                    {
                        UserName = email.Split('@')[0],
                        Email = email,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    var result = await userManager.CreateAsync(user, "Password123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Customer");
                    }
                }
            }

            var sellers = await userManager.GetUsersInRoleAsync("Seller");
            var customers = await userManager.GetUsersInRoleAsync("Customer");

            // 3. Ensure Products exist (at least 14)
            if (!await context.Products.AnyAsync() && sellers.Any() && allCategories.Any())
            {
                var random = new Random();
                var products = new List<Product>
                {
                    new Product { Name = "MacBook Pro M3", Description = "Latest Apple laptop with M3 chip and Retina display.", Price = 1999.99m, Stock = 10, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                    new Product { Name = "Dell XPS 15", Description = "Powerful Windows laptop with stunning OLED screen.", Price = 1750.00m, Stock = 15, CreatedAt = DateTime.UtcNow.AddDays(-9) },
                    new Product { Name = "iPhone 15 Pro", Description = "Titanium build, A17 Pro chip, best-in-class camera.", Price = 999.00m, Stock = 25, CreatedAt = DateTime.UtcNow.AddDays(-8) },
                    new Product { Name = "Samsung Galaxy S24 Ultra", Description = "AI-powered smartphone with 200MP camera.", Price = 1299.99m, Stock = 20, CreatedAt = DateTime.UtcNow.AddDays(-7) },
                    new Product { Name = "Sony WH-1000XM5", Description = "Industry leading noise canceling headphones.", Price = 349.99m, Stock = 30, CreatedAt = DateTime.UtcNow.AddDays(-6) },
                    new Product { Name = "AirPods Max", Description = "Premium over-ear headphones by Apple.", Price = 549.00m, Stock = 12, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                    new Product { Name = "PlayStation 5", Description = "Next-gen gaming console for 4K gaming.", Price = 499.99m, Stock = 8, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                    new Product { Name = "Xbox Series X", Description = "The fastest, most powerful Xbox ever.", Price = 499.00m, Stock = 5, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                    new Product { Name = "Nintendo Switch OLED", Description = "Handheld gaming with a vibrant OLED screen.", Price = 349.99m, Stock = 15, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                    new Product { Name = "RTX 4090 GPU", Description = "The ultimate graphics card for PC gaming.", Price = 1599.00m, Stock = 3, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                    new Product { Name = "Logitech G Pro X Superlight", Description = "Ultra-lightweight wireless gaming mouse.", Price = 149.99m, Stock = 40, CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Keychron Q1 Pro", Description = "Full-metal custom mechanical keyboard.", Price = 199.00m, Stock = 20, CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Dyson V15 Detect", Description = "The most powerful, intelligent cordless vacuum.", Price = 749.99m, Stock = 7, CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Ember Mug 2", Description = "Smart mug that keeps your coffee at the perfect temp.", Price = 129.00m, Stock = 50, CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Kindle Paperwhite", Description = "Best e-reader with adjustable warm light.", Price = 139.99m, Stock = 100, CreatedAt = DateTime.UtcNow }
                };

                foreach (var product in products)
                {
                    // Assign random category
                    product.CategoryId = allCategories[random.Next(allCategories.Count)].Id;
                    // Assign random seller
                    product.SellerId = sellers[random.Next(sellers.Count)].Id;
                }

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }

            // 4. Ensure Reviews exist
            if (!await context.Reviews.AnyAsync() && customers.Any())
            {
                var allProducts = await context.Products.ToListAsync();
                var random = new Random();
                var reviewComments = new[]
                {
                    "Excellent product, highly recommend!",
                    "Good value for money.",
                    "Build quality is amazing.",
                    "Fast shipping and works as expected.",
                    "A bit expensive but worth it.",
                    "Could be better, but still decent.",
                    "Fantastic customer support!",
                    "Best purchase I've made this year.",
                    "Love the design and features."
                };

                var reviews = new List<Review>();

                foreach (var product in allProducts)
                {
                    // Add 1-2 random reviews per product
                    int numReviews = random.Next(1, 3);
                    for (int i = 0; i < numReviews; i++)
                    {
                        var customer = customers[random.Next(customers.Count)];
                        reviews.Add(new Review
                        {
                            ProductId = product.Id,
                            UserId = customer.Id,
                            Rating = random.Next(4, 6), // Mostly 4-5 stars
                            Comment = reviewComments[random.Next(reviewComments.Length)],
                            CreatedAt = DateTime.UtcNow.AddHours(-random.Next(1, 100))
                        });
                    }
                }

                await context.Reviews.AddRangeAsync(reviews);
                await context.SaveChangesAsync();
            }
        }
    }
}
