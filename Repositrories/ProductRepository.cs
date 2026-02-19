using E_Commerce.Data;
using E_Commerce.DTOs.ProductDTO;
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

       

        public async Task<PagedResult<Product>> GetProductsAsync(ProductQueryParameters queryParameters)
        {
            var query = _dbContext.Products
            .Include(p => p.Category)  // Include category details
            .AsNoTracking()
            .AsQueryable();

            // Apply filters
            query = ApplyFilters(query, queryParameters);

            // Apply sorting
            query = ApplySorting(query, queryParameters);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                TotalCount = totalCount,
                Page = queryParameters.Page,
                PageSize = queryParameters.PageSize
            };
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




        private IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductQueryParameters parameters)
        {
            // Filter by name (partial match)
            if (!string.IsNullOrWhiteSpace(parameters.Name))
            {
                query = query.Where(p => p.Name.Contains(parameters.Name));
            }

            // Filter by category
            if (parameters.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == parameters.CategoryId.Value);
            }

            // Filter by price range
            if (parameters.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= parameters.MinPrice.Value);
            }

            if (parameters.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= parameters.MaxPrice.Value);
            }

            // Filter by seller
            if (parameters.SellerId.HasValue)
            {
                query = query.Where(p => p.SellerId == parameters.SellerId.Value);
            }

            // Filter by stock availability
            if (parameters.InStockOnly)
            {
                query = query.Where(p => p.Stock > 0);
            }

            // Search term (search in name and description)
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm));
            }

            return query;
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, ProductQueryParameters parameters)
        {
            var sortBy = parameters.SortBy?.ToLower() ?? "name";
            var isDescending = parameters.SortDescending;

            return sortBy switch
            {
                "price" => isDescending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "createdat" or "date" => isDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                "name" or _ => isDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name)
            };
        }
        public override async Task DeleteAsync(int id)
        {
            var product = await _dbContext.Products
                .Include(p => p.OrderItems)
                    .ThenInclude(oi => oi.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                // Handle related order items and their parent orders
                if (product.OrderItems != null && product.OrderItems.Any())
                {
                    // Group order items by order to update totals efficiently
                    var ordersToUpdate = product.OrderItems
                        .Where(oi => oi.Order != null)
                        .GroupBy(oi => oi.Order);

                    foreach (var group in ordersToUpdate)
                    {
                        var order = group.Key;
                        var itemsToRemove = group.ToList();

                        // Subtract price from order total
                        foreach (var item in itemsToRemove)
                        {
                            order.TotalAmount -= (item.UnitPrice * item.Quantity);
                            _dbContext.OrderItems.Remove(item);
                        }

                        // If total amount is 0 or less, or no items left, delete the order
                        // Note: We need to check if there are other items in the order not related to this product
                        var otherItemsExist = await _dbContext.OrderItems
                            .AnyAsync(oi => oi.OrderId == order.Id && oi.ProductId != id);

                        if (!otherItemsExist || order.TotalAmount <= 0)
                        {
                            _dbContext.Orders.Remove(order);
                        }
                    }
                }

                // Finally delete the product
                _dbContext.Products.Remove(product);
            }
        }
    }
}
