using E_Commerce.Models;

namespace E_Commerce.Repositrories.Interfaces
{
    public interface ICartItemRepository : IGenericRepository<CartItem>
    {
        Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(Guid userId);

        //delete whole cart, meaning all cart items for a user
        Task DeleteWholeCartByUserIdAsync(Guid userId);
    }
}
