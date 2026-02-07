using E_Commerce.Data;
using E_Commerce.Models;
using E_Commerce.Repositrories.E_Commerce.Repositories;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repositrories
{
    public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CartItemRepository> _logger;

        public CartItemRepository(ApplicationDbContext dbContext,ILogger<CartItemRepository> logger) : base(dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }


        public async Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(Guid userId)
        {
        
            var cartItems = await _dbContext.CartItems
                .Include(ci => ci.Product)
                .AsNoTracking()
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} cart items for user {UserId}", cartItems.Count, userId);

            return cartItems;

        }

        public override async Task<CartItem> AddAsync(CartItem cartItem)
        {
            // Check if user exists
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id.ToString() == cartItem.UserId.ToString());
            _logger.LogInformation("Adding cart item for user {UserId} with product {ProductId} and quantity {Quantity}", cartItem.UserId, cartItem.ProductId, cartItem.Quantity);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {cartItem.UserId} not found.");
            }

            if (cartItem.ProductId > 0) 
            {
                var product = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);

                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {cartItem.ProductId} not found.");
                }
            }

            var existingCartItem = await _dbContext.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.UserId.ToString() == cartItem.UserId.ToString() &&
                    ci.ProductId == cartItem.ProductId);

            if (existingCartItem != null)
            {
                // Update quantity instead of adding new
                existingCartItem.Quantity += cartItem.Quantity;
                _dbContext.CartItems.Update(existingCartItem);
                _logger.LogInformation("Existing cart item found for user {UserId} and product {ProductId}. Updated quantity to {Quantity}.", cartItem.UserId, cartItem.ProductId, existingCartItem.Quantity);
                return existingCartItem;
            }

            _logger.LogInformation("No existing cart item found for user {UserId} and product {ProductId}. Adding new cart item.", cartItem.UserId, cartItem.ProductId);
            // Add new cart item
            return await base.AddAsync(cartItem);
        }

        public async Task DeleteWholeCartByUserIdAsync(Guid userId)
        {
            //check user exists
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId.ToString());
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
            var cartItems = await _dbContext.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync();
            if (cartItems.Any())
            {
                _dbContext.CartItems.RemoveRange(cartItems);
               _logger.LogInformation("Deleted {Count} cart items for user {UserId}", cartItems.Count, userId);
               
            }
        }
    }
}
