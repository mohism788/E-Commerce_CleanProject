using E_Commerce.Data;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repositrories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddProductAsync(Product product)
        {
            //check if product is null
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null");
            }
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with id {id} not found");
            }
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();


        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _dbContext.Products.ToListAsync();

        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with id {id} not found");
            }
            return product;

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

        public async Task UpdateProductAsync(Product product)
        {
            

            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
        }
    }
}
