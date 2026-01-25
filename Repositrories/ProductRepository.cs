using E_Commerce.Data;
using E_Commerce.Models;
using E_Commerce.Repositrories.E_Commerce.Repositories;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repositrories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Product>> GetAllProductInCategoryAsync(int categoryId)
        {
                var category = await _dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(c=>c.Id == categoryId);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with id {categoryId} not found");
            }
            var products = await _dbContext.Products
                .AsNoTracking()
                .Where(p=>p.CategoryId== categoryId)
                .ToListAsync();

            return products;

        }

        public async Task<IEnumerable<Product>> GetProductsBySellerIdAsync(Guid sellerId)
        {

            var seller = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id.ToString() == sellerId.ToString());
            if (seller == null)
            {
                throw new KeyNotFoundException($"Seller with id {sellerId} not found");
            }

            var products = await _dbContext.Products
                .AsNoTracking()
                .Where(p => p.SellerId == sellerId)
                .ToListAsync();

            return products;
        }

        public async Task<Guid> GetSellerIdByProductIdAsync(int productId)
        {
            var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with id {productId} not found");
            }
            return product.SellerId;
        }

 
    }
}
