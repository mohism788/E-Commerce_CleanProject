using System.Security.Claims;
using AutoMapper;
using E_Commerce.DTOs.CartItemDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
using E_Commerce.Repositrories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CartItemsController> _logger;

        public CartItemsController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CartItemsController> logger)
        {

            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        //get all cart items

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
               _logger.LogInformation("Admin {AdminId} is retrieving all cart items", GetCurrentUserId());
                var cartItems = await _unitOfWork.CartItems.GetAllAsync();

                _logger.LogInformation("Retrieved all cart items successfully.");

                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while retrieving cart items: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //Get cart items by userId
        [HttpGet("{userId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCartItemsByUserId(Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Ensure user can only add items to their own cart
                if (userId != currentUserId)
                {
                    return Forbid(); // or BadRequest("Cannot add items to another user's cart");
                }
                _logger.LogInformation("Customer {CustomerId} is retrieving cart items for user {UserId}", currentUserId, userId);
                var cartItems = await _unitOfWork.CartItems.GetCartItemsByUserIdAsync(userId);
                _logger.LogInformation("Retrieved cart items for user {UserId} successfully.", userId);
                return Ok(cartItems);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while retrieving cart items: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //create cart item
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateCartItem([FromBody] CreateCartItemDto createCartItemDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Ensure user can only add items to their own cart
                if (createCartItemDto.userId != currentUserId)
                {
                    return Forbid(); // or BadRequest("Cannot add items to another user's cart");
                }

                return await _unitOfWork.ExecuteResultStrategyAsync<IActionResult>(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        _logger.LogInformation("Customer {CustomerId} is adding a new cart item for user {UserId}", currentUserId, createCartItemDto.userId);

                        // Stock Check
                        var product = await _unitOfWork.Products.GetByIdAsync(createCartItemDto.ProductId);
                        if (product == null)
                        {
                            return NotFound(new { success = false, message = "Product not found" });
                        }

                        if (product.Stock == 0)
                        {
                            return BadRequest(new { success = false, message = "Sorry, this product is currently out of stock." });
                        }

                        if (product.Stock < createCartItemDto.Quantity)
                        {
                            return BadRequest(new { success = false, message = $"Sorry, only {product.Stock} units of {product.Name} are available." });
                        }

                        var cartItem = _mapper.Map<CartItem>(createCartItemDto);
                        await _unitOfWork.CartItems.AddAsync(cartItem);
                        _logger.LogInformation("Cart item added to database context for user {UserId}", createCartItemDto.userId);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        _logger.LogInformation("Cart item added successfully for user {UserId}", createCartItemDto.userId);
                        return StatusCode(201, new
                        {
                            success = true,
                            message = "Cart item Added successfully"
                        });
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError("An error occurred while adding a cart item for user {UserId} by customer {CustomerId}. Transaction rolled back.", createCartItemDto.userId, currentUserId);
                        throw;
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while creating the cart item: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //delete cart item
        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                return await _unitOfWork.ExecuteResultStrategyAsync<IActionResult>(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(id);
                        _logger.LogInformation("Customer {CustomerId} is attempting to delete cart item with id {CartItemId}", currentUserId, id);
                        // Ensure user can only delete items from their own cart
                        if (cartItem.UserId != currentUserId)
                        {
                            return Forbid(); // or return Unauthorized();
                        }

                        if (cartItem == null)
                        {
                            return NotFound(new { success = false, message = "Cart item not found" });
                        }


                        await _unitOfWork.CartItems.DeleteAsync(id);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        _logger.LogInformation("Cart item with id {CartItemId} deleted successfully for user {CustomerId}", id, currentUserId);
                        return Ok(new { success = true, message = "Cart item deleted successfully" });
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError("An error occurred while deleting cart item with id {CartItemId} for user {CustomerId}. Transaction rolled back.", id, currentUserId);
                        throw;
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while deleting the cart item: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //delete whole cart by userId
        [HttpDelete("user/{userId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteWholeCartByUserId(Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Ensure user can only view their own cart
                if (currentUserId != userId)
                {
                    return Forbid(); // or return Unauthorized();
                }
                _logger.LogInformation("Customer {CustomerId} is attempting to delete the whole cart for user {UserId}", currentUserId, userId);
                return await _unitOfWork.ExecuteResultStrategyAsync<IActionResult>(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {

                        await _unitOfWork.CartItems.DeleteWholeCartByUserIdAsync(userId);
                        _logger.LogInformation("Cart items for user {UserId} marked for deletion in database context", userId);

                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        _logger.LogInformation("Whole cart deleted successfully for user {UserId} by customer {CustomerId}", userId, currentUserId);

                        return Ok(new { success = true, message = "Whole cart deleted successfully" });
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError("An error occurred while deleting the whole cart for user {UserId} by customer {CustomerId}. Transaction rolled back.", userId, currentUserId);
                        throw;
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while deleting the whole cart: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }


        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return userId;
        }

    }
}
