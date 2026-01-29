using E_Commerce.Data;
using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.E_Commerce.Repositories;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repositrories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public OrderRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        
        public async Task<Order> CheckoutAsync(Guid userId)
        {
            var cartItems = await _context.CartItems
                                          .Include(ci => ci.Product) 
                                          .Where(ci => ci.UserId == userId)
                                          .ToListAsync();
            if (cartItems == null || !cartItems.Any())
            {
                throw new InvalidOperationException("No items in the cart to checkout.");
            }
            //claculate total amount 
            var _totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = _totalAmount,
                Status = "Pending",
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.Price
                }).ToList()
            };
            await _dbContext.Orders.AddAsync(order);
            _dbContext.CartItems.RemoveRange(cartItems);
            //decrease product stock
            foreach (var item in cartItems)
            {
                var product = await _dbContext.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                }
            }
            await _dbContext.SaveChangesAsync();
            return order;


        }



        public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId)
        {
            return await _dbContext.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(p=>p.Product)
                .Where(o => o.UserId.ToString() == userId.ToString())
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();



        }

        //override GetByIdAsync
        public override async Task<Order?> GetByIdAsync(int id)
        {
            if (await _dbContext.Orders.FindAsync(id) != null)
            {
                return await _dbContext.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);
            }
            else
            {
                throw new KeyNotFoundException($"Order with id {id} not found");
            }
        }

        public async Task<Order> BuyNowAsync(Guid userId, BuyNowOrderItemDto buyNowOrderItemDto)
        { 
            var product = await _dbContext.Products.FindAsync(buyNowOrderItemDto.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with id {buyNowOrderItemDto.ProductId} not found");
            }
            var totalAmount = buyNowOrderItemDto.Quantity * product.Price;

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
            await _dbContext.Orders.AddAsync(order);
            //decrease product stock
            product.Stock -= buyNowOrderItemDto.Quantity;
            await _dbContext.SaveChangesAsync();
            return order;


        }
        
    }
    
}
