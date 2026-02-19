using E_Commerce.Data;
using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.E_Commerce.Repositories;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repositrories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        // Get user orders with items and products
        public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId)
        {
            return await _context.Orders
                .AsNoTracking().Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // Buy Now - creates order from single product
        public async Task<Order> BuyNowAsync(Guid userId, BuyNowOrderItemDto buyNowOrderItemDto)
        {
            // Get product
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == buyNowOrderItemDto.ProductId);

            if (product == null)
            {
                throw new KeyNotFoundException($"Product with id {buyNowOrderItemDto.ProductId} not found");
            }

            // Calculate total
            var totalAmount = buyNowOrderItemDto.Quantity * product.Price;

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = "Pending",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = buyNowOrderItemDto.ProductId,
                        Quantity = buyNowOrderItemDto.Quantity,
                        UnitPrice = product.Price
                    }
                }
            };

            // Add order (doesn't save - UnitOfWork will handle SaveChanges)
            await AddAsync(order);
            return order;
        }

        // Create order from cart items
        public async Task<Order> CreateOrderFromCartAsync(Guid userId, IEnumerable<CartItem> cartItems)
        {
            // Calculate total amount
            var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = "Pending",
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.Price
                }).ToList()
            };


            await AddAsync(order);
            return order;
        }

        // Get cart items with product details for a user
        public async Task<List<CartItem>> GetCartItemsWithProductsAsync(Guid userId)
        {
            return await _context.CartItems
                .AsNoTracking().Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();
        }

        // Remove cart items (mark for deletion)
        public async Task RemoveCartItemsAsync(IEnumerable<CartItem> cartItems)
        {
            _context.CartItems.RemoveRange(cartItems);
        }

        // Update product stock (increase or decrease)
        public async Task UpdateProductStockAsync(int productId, int quantityChange)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.Stock += quantityChange; // Positive = increase, Negative = decrease
            }
        }

        // ========== OPTIONAL: Additional helper methods ==========

        // Get order by ID with details (override from GenericRepository)
        public override async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
              .AsNoTracking().Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // Update order status
        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
            }
        }

        // Get order details with all relationships
        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _context.Orders
                .AsNoTracking().Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        // Get order items for an order
        public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId)
        {
            return await _context.OrderItems
                .AsNoTracking().Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        // Check if user has any orders
        public async Task<bool> UserHasOrdersAsync(Guid userId)
        {
            return await _context.Orders.AsNoTracking().AnyAsync(o => o.UserId == userId);
        }

        // Get recent orders for a user
        public async Task<List<Order>> GetRecentUserOrdersAsync(Guid userId, int count = 5)
        {
            return await _context.Orders
                .AsNoTracking().Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        // Calculate total spent by user
        public async Task<decimal> GetTotalSpentByUserAsync(Guid userId)
        {
            return await _context.Orders
               .AsNoTracking().Where(o => o.UserId == userId && o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);
        }


        //override delete to handle cascade delete of order items
        public override async Task DeleteAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                // Remove related order items first
                 _context.OrderItems.RemoveRange(order.OrderItems);
                // Then remove the order
                _context.Orders.Remove(order);
                //return products to stock
                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(orderItem.ProductId);
                    if (product != null)
                    {
                        product.Stock += orderItem.Quantity;
                    }
                }
            }
        }

        
    }
}