using System.Security.Claims;
using AutoMapper;
using E_Commerce.DTOs.CartItemDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemsController : ControllerBase
    {
        private readonly ICartItemRepository _cartItemRepo;
        private readonly IMapper _mapper;

        public CartItemsController(ICartItemRepository cartItemRepo, IMapper mapper)
        {
            _cartItemRepo = cartItemRepo;
            _mapper = mapper;
        }

        //get all cart items

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
               
                var cartItems = await _cartItemRepo.GetAllAsync();

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

                var cartItems = await _cartItemRepo.GetCartItemsByUserIdAsync(userId);
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

                var cartItem = _mapper.Map<CartItem>(createCartItemDto);
                await _cartItemRepo.AddAsync(cartItem);
                return StatusCode(201, new
                {
                            success = true, 
                            message = "Cart item Added successfully" 
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
                var cartItem = await _cartItemRepo.GetByIdAsync(id);
                
                // Ensure user can only delete items from their own cart
                if (cartItem.UserId != currentUserId)
                {
                    return Forbid(); // or return Unauthorized();
                }

                if (cartItem == null)
                {
                    return NotFound(new { success = false, message = "Cart item not found" });
                }
                

                await _cartItemRepo.DeleteAsync(id);
                return Ok(new { success = true, message = "Cart item deleted successfully" });
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

                await _cartItemRepo.DeleteWholeCartByUserIdAsync(userId);
                return Ok(new { success = true, message = "Whole cart deleted successfully" });
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
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId" ||
                                                              c.Type == ClaimTypes.NameIdentifier ||
                                                              c.Type == "sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return userId;
        }

    }
}
