using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Models;

namespace E_Commerce.Repositrories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {

        Task<Order> CheckoutAsync(Guid userId);
        Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId);

        //buy now
        Task<Order> BuyNowAsync(Guid userId, BuyNowOrderItemDto buyNowOrderItemDto);




    }
}
