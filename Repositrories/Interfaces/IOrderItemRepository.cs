using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Models;

namespace E_Commerce.Repositrories.Interfaces
{
    public interface IOrderItemRepository : IGenericRepository<OrderItem>
    {
        public Task<OrderItem> AddOrderItemAsync(CreateOrderItemDto dto, Guid? userId = null);

        
        


    }
}
