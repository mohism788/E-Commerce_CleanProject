using E_Commerce.Data;
using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.E_Commerce.Repositories;
using E_Commerce.Repositrories.Interfaces;
using E_Commerce.Repositrories.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repositrories
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        private readonly ApplicationDbContext _dbContext;
        

        public OrderItemRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OrderItem> AddOrderItemAsync(CreateOrderItemDto dto, Guid? userId = null)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.ProductId <= 0)
                throw new ArgumentException("ProductId must be valid");

            if (dto.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");

            Order order;

            // If OrderId is provided, use existing order
            if (dto.OrderId.HasValue && dto.OrderId > 0)
            {
                order = await _dbContext.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == dto.OrderId.Value);

                if (order == null)
                    throw new Exception($"Order with ID {dto.OrderId.Value} not found");

                // Optional: Verify user owns this order
                // if (userId.HasValue && order.UserId != userId.Value)
                //     throw new UnauthorizedAccessException("User does not own this order");
            }
            else
            {
                // Create a new order if no OrderId provided
                if (!userId.HasValue)
                    throw new ArgumentException("UserId is required when creating a new order");

                order = new Order
                {
                    UserId = userId.Value,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending", // Or "Draft", "Cart", etc.
                    OrderItems = new List<OrderItem>()
                };

                await _dbContext.Orders.AddAsync(order);
                //await _unitOfWork.SaveChangesAsync(); // Save to get OrderId
            }

            // Get product details
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                throw new Exception($"Product with ID {dto.ProductId} not found");

            // Check stock availability
            if (product.Stock < dto.Quantity)
                throw new Exception($"Insufficient stock. Available: {product.Stock}");

            // Check if product already exists in the order
            var existingOrderItem = order.OrderItems?
                .FirstOrDefault(oi => oi.ProductId == dto.ProductId);

            if (existingOrderItem != null)
            {
                // Update existing order item
                var newTotalQuantity = existingOrderItem.Quantity + dto.Quantity;

                // Validate total quantity doesn't exceed stock
                if (product.Stock< newTotalQuantity)
                    throw new Exception($"Insufficient stock for additional quantity. Available: {product.Stock}");

                existingOrderItem.Quantity = newTotalQuantity;
                existingOrderItem.UnitPrice = product.Price; // Update with current price

                // Update order total
                order.TotalAmount = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
                order.OrderDate = DateTime.UtcNow;

                //await _unitOfWork.SaveChangesAsync();
                return existingOrderItem;
            }
            else
            {
                // Create new order item
                var newOrderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    UnitPrice = product.Price,
                };

                await _dbContext.OrderItems.AddAsync(newOrderItem);

                // Update order total
                order.TotalAmount = order.TotalAmount  + (product.Price * dto.Quantity);
                order.OrderDate = DateTime.UtcNow;

                //await _unitOfWork.SaveChangesAsync();
                return newOrderItem;
            }
        }


        public override async Task DeleteAsync(int id)
        {
            var orderItem = await _dbContext.OrderItems
                                   .Include(oi => oi.Order)
                                   .ThenInclude(o => o.OrderItems)
                                   .FirstOrDefaultAsync(oi => oi.Id == id);
            if (orderItem != null)
            {
                var order = orderItem.Order;
                
                    bool isLastItem = false;
                    if (order != null && order.OrderItems != null)
                    {
                        
                        var itemsInOrder = order.OrderItems.Where(oi => oi.Id != id).ToList();
                        isLastItem = itemsInOrder.Count == 0;
                    }
                    _dbContext.OrderItems.Remove(orderItem);
                    //return quantity to stock
                    var product = await _dbContext.Products.FindAsync(orderItem.ProductId);
                    if (product != null)
                    {
                        product.Stock += orderItem.Quantity;
                    }
                    if (isLastItem && order != null)
                    {
                        _dbContext.Orders.Remove(order);
                    }
                
            }
        }



        public override async Task<OrderItem?> GetByIdAsync(int id)
        {
            // Add logging here too
            Console.WriteLine($"Repository: Getting OrderItem with id {id}");

            var orderItem = await _dbContext.OrderItems
                .Include(oi => oi.Order) // Make sure this line is there
                .AsNoTracking() // Remove this if present - it can prevent loading
                .FirstOrDefaultAsync(oi => oi.Id == id);

            Console.WriteLine($"Repository: OrderItem found: {orderItem != null}");
            if (orderItem != null)
            {
                Console.WriteLine($"Repository: OrderItem.Order: {orderItem.Order}");
                Console.WriteLine($"Repository: OrderItem.OrderId: {orderItem.OrderId}");
            }

            return orderItem;
        }

    }
}
