using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Models;

namespace E_Commerce.Services.Interfaces
{
    public interface IOrderItemService
    {
        Task<OrderItem> AddOrderItemAsync(CreateOrderItemDto dto, Guid userId);
        Task RemoveItemFromOrderAsync(int orderItemId);
    }
}
