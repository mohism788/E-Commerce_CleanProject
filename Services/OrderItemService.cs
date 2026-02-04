using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Models;
using E_Commerce.Services.Interfaces;

namespace E_Commerce.Services
{
    public class OrderItemService : IOrderItemService
    {
        public Task<OrderItem> AddOrderItemAsync(CreateOrderItemDto dto, Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveItemFromOrderAsync(int orderItemId)
        {
            throw new NotImplementedException();
        }
    }
}
