using E_Commerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            try
            {
                using var context = new ApplicationDbContext(
                    serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

                // =====================================================
                // 1. ENSURE CATEGORIES EXIST
                // =====================================================

                var berserkCategories = new List<string>
                {
                    "Peripherals",
                    "Monitors",
                    "PC Components",
                    "Laptops",
                    "Accessories"
                };

                foreach (var catName in berserkCategories)
                {
                    if (!await context.Categories.AnyAsync(c => c.Name == catName))
                    {
                        await context.Categories.AddAsync(new Category
                        {
                            Name = catName
                        });
                    }
                }

                await context.SaveChangesAsync();


                // =====================================================
                // 2. SEED PRODUCTS ONLY IF DB IS EMPTY
                // =====================================================

                bool hasAnyProducts = await context.Products.AnyAsync();

                if (hasAnyProducts)
                {
                    Console.WriteLine("Database already contains products — skipping seed.");
                    return;
                }


                // =====================================================
                // 3. GET SELLERS
                // =====================================================

                var userManager =
                    serviceProvider.GetRequiredService<UserManager<User>>();

                var sellers =
                    await userManager.GetUsersInRoleAsync("Seller");

                if (!sellers.Any())
                {
                    Console.WriteLine("No sellers found — skipping product seeding.");
                    return;
                }


                // =====================================================
                // 4. SEED REAL PRODUCTS
                // =====================================================

                var categoriesToSeed = await context.Categories
                    .Where(c => berserkCategories.Contains(c.Name))
                    .ToListAsync();

                var products = new List<Product>();
                var random = new Random();


                foreach (var category in categoriesToSeed)
                {
                    switch (category.Name)
                    {
                        case "Peripherals":

                            products.Add(new Product
                            {
                                Name = "Mechanical Keyboard RGB",
                                Description = "High-performance mechanical keyboard with customizable RGB lighting.",
                                Price = 129.99m,
                                Stock = 50,
                                SellerId = sellers[random.Next(sellers.Count)].Id,
                                CategoryId = category.Id,
                                CreatedAt = DateTime.UtcNow
                            });

                            products.Add(new Product
                            {
                                Name = "Gaming Mouse Pro",
                                Description = "Ultra-lightweight gaming mouse with high DPI sensor.",
                                Price = 79.99m,
                                Stock = 100,
                                SellerId = sellers[random.Next(sellers.Count)].Id,
                                CategoryId = category.Id,
                                CreatedAt = DateTime.UtcNow
                            });

                            break;


                        case "Monitors":

                            products.Add(new Product
                            {
                                Name = "27\" Limited Edition Monitor",
                                Description = "144Hz refresh rate, 1ms response time, IPS panel.",
                                Price = 299.99m,
                                Stock = 30,
                                SellerId = sellers[random.Next(sellers.Count)].Id,
                                CategoryId = category.Id,
                                CreatedAt = DateTime.UtcNow
                            });

                            break;


                        case "PC Components":

                            products.Add(new Product
                            {
                                Name = "RTX 4090 Graphics Card",
                                Description = "Next-gen graphics performance for 4K gaming.",
                                Price = 1599.99m,
                                Stock = 5,
                                SellerId = sellers[random.Next(sellers.Count)].Id,
                                CategoryId = category.Id,
                                CreatedAt = DateTime.UtcNow
                            });

                            break;


                        case "Laptops":

                            products.Add(new Product
                            {
                                Name = "Berserk blade 15",
                                Description = "Thin and light gaming laptop with powerful specs.",
                                Price = 1999.99m,
                                Stock = 10,
                                SellerId = sellers[random.Next(sellers.Count)].Id,
                                CategoryId = category.Id,
                                CreatedAt = DateTime.UtcNow
                            });

                            break;


                        case "Accessories":

                            products.Add(new Product
                            {
                                Name = "Large Gaming Mousepad",
                                Description = "Smooth surface for precise mouse control.",
                                Price = 29.99m,
                                Stock = 150,
                                SellerId = sellers[random.Next(sellers.Count)].Id,
                                CategoryId = category.Id,
                                CreatedAt = DateTime.UtcNow
                            });

                            break;
                    }
                }


                if (products.Any())
                {
                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();

                    Console.WriteLine("Products seeded successfully.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seeding failed: {ex.Message}");
            }
        }
    }
}
