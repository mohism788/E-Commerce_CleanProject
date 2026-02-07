using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

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

                // Seed Categories if empty
                if (!await context.Categories.AnyAsync())
                {
                    var categories = new List<Category>
                    {
                        new Category { Name = "Electronics" },
                        new Category { Name = "Clothing" },
                        new Category { Name = "Books" },
                        new Category { Name = "Home & Garden" }
                    };
                    await context.Categories.AddRangeAsync(categories);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seeding failed: {ex.Message}");
            }
        }
    }
}
