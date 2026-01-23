using E_Commerce.Data;
using E_Commerce.Models;
using E_Commerce.Repositrories.E_Commerce.Repositories;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repositrories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        //override create method to check for duplicate category names
        public override async Task<Category> AddAsync(Category entity)
        {
            var existingCategory = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == entity.Name.ToLower());
            if (existingCategory != null)
            {
                throw new InvalidOperationException($"A category with the name '{entity.Name}' already exists.");
            }
            return await base.AddAsync(entity);
        }
    }
}
