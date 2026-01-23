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
        public CartItemRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(Guid userId)
        {
        
            var cartItems = await _dbContext.CartItems
                .AsNoTracking()
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            Console.WriteLine($"Query returned: {cartItems.Count} items");

            return cartItems;

        }

        public override async Task<CartItem> AddAsync(CartItem cartItem)
        {
            // Check if user exists
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id.ToString() == cartItem.UserId.ToString());

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
                await _dbContext.SaveChangesAsync();
                return existingCartItem;
            }

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
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
