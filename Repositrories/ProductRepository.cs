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
