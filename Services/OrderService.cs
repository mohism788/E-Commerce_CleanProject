// Services/OrderService.cs
using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.UnitOfWork;
using E_Commerce.Services.Interfaces;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Order> CheckoutAsync(Guid userId)
    {
        return await _unitOfWork.ExecuteResultStrategyAsync<Order>(async () =>
        {
            // Start transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Get cart items using OrderRepository helper
                var cartItems = await _unitOfWork.Orders.GetCartItemsWithProductsAsync(userId);

                if (!cartItems.Any())
                    throw new InvalidOperationException("Cart is empty");

                // 2. Validate stock
                foreach (var item in cartItems)
                {
                    if (item.Product.Stock == 0)
                        throw new InvalidOperationException($"Sorry, {item.Product.Name} is currently out of stock.");

                    if (item.Product.Stock < item.Quantity)
                        throw new InvalidOperationException($"Sorry, only {item.Product.Stock} units of {item.Product.Name} are available.");
                }

                // 3. Create order (using OrderRepository helper)
                var order = await _unitOfWork.Orders.CreateOrderFromCartAsync(userId, cartItems);

                // 4. Update stock for each product (using OrderRepository helper)
                foreach (var item in cartItems)
                {
                    await _unitOfWork.Orders.UpdateProductStockAsync(item.ProductId, -item.Quantity);
                }

                // 5. Clear cart (using OrderRepository helper)
                await _unitOfWork.Orders.RemoveCartItemsAsync(cartItems);

                // 6. Save all changes through UnitOfWork
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return order;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task<Order> BuyNowAsync(Guid userId, BuyNowOrderItemDto dto)
    {
        return await _unitOfWork.ExecuteResultStrategyAsync<Order>(async () =>
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Get and validate product
                var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
                if (product == null)
                    throw new KeyNotFoundException($"Product not found");

                if (product.Stock == 0)
                    throw new InvalidOperationException("Sorry, this product is currently out of stock.");

                if (product.Stock < dto.Quantity)
                    throw new InvalidOperationException($"Sorry, only {product.Stock} units of {product.Name} are available.");

                // 2. Create order (using OrderRepository)
                var order = await _unitOfWork.Orders.BuyNowAsync(userId, dto);

                // 3. Update stock
                await _unitOfWork.Orders.UpdateProductStockAsync(dto.ProductId, -dto.Quantity);

                // 4. Save
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return order;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        });
    }
}
