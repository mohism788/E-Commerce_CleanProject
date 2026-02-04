using System.Security.Claims;
using AutoMapper;
using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.DTOs.ReviewDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemRepository _orderItemRepo;
        private readonly IMapper _mapper;
        public OrderItemController(IOrderItemRepository orderItemRepo, IMapper mapper)
        {
            _orderItemRepo = orderItemRepo;
            _mapper = mapper;
        }

        //get all order items
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderItems()
        {
            try
            {
                var orderItems = await _orderItemRepo.GetAllAsync();
               var orderItemDtos = _mapper.Map<IEnumerable<OrderItemDto>>(orderItems);


                return Ok(orderItemDtos);
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while retrieving order items: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //add order item
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrderItem([FromBody] CreateOrderItemDto createOrderItemDto, Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                // Ensure user can only add items to their own order
                if (userId != currentUserId)
                {
                    return Forbid(); // or BadRequest("Cannot add items to another user's order");
                }

                await _orderItemRepo.AddOrderItemAsync(createOrderItemDto);

                var orderItem = _mapper.Map<OrderItem>(createOrderItemDto);
                var orderItemDto = _mapper.Map<OrderItemDto>(orderItem);
                return CreatedAtAction(nameof(GetOrderItems), new { id = orderItem.Id }, orderItemDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while creating the order item: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //delete order item
        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                
                var orderItem = await _orderItemRepo.GetByIdAsync(id);

                if (orderItem == null)
                {
                    return NotFound($"Order item with id {id} not found");
                }

                // Ensure user can only delete items from their own order
                if (orderItem.Order.UserId != currentUserId)
                {
                    return Forbid(); // or return Unauthorized();
                }

                await _orderItemRepo.DeleteAsync(orderItem.Id);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while deleting the order item: {ex.Message}")
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
