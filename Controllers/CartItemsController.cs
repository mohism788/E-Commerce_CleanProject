using AutoMapper;
using E_Commerce.DTOs.CartItemDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
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
        public async Task<IActionResult> GetCartItemsByUserId(Guid userId)
        {
            try
            {
                var cartItems = await _cartItemRepo.GetCartItemsByUserIdAsync(userId);
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

        //create cart item
        [HttpPost]
        public async Task<IActionResult> CreateCartItem([FromBody] CreateCartItemDto createCartItemDto)
        {
            try
            {
                var cartItem = _mapper.Map<CartItem>(createCartItemDto);
                await _cartItemRepo.AddAsync(cartItem);
                return StatusCode(201, new
                {
                            success = true, 
                            message = "Cart item Added successfully" 
                });
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
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            try
            {
                var exists = await _cartItemRepo.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound(new { success = false, message = "Cart item not found" });
                }
                await _cartItemRepo.DeleteAsync(id);
                return Ok(new { success = true, message = "Cart item deleted successfully" });
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
        public async Task<IActionResult> DeleteWholeCartByUserId(Guid userId)
        {
            try
            {
                await _cartItemRepo.DeleteWholeCartByUserIdAsync(userId);
                return Ok(new { success = true, message = "Whole cart deleted successfully" });
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while deleting the whole cart: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }



    }
}
