using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Models;

namespace E_Commerce.Services.Interfaces
{
    public interface IOrderService
    {
        public Task<Order> CheckoutAsync(Guid userId);
        public Task<Order> BuyNowAsync(Guid userId, BuyNowOrderItemDto dto);
    }
}
