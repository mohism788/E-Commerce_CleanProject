using System.Security.Claims;
using AutoMapper;
using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public OrderItemController(IUnitOfWork unitOfWork, IMapper mapper)
        {

            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //get all order items
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrderItems()
        {
            try
            {
                var orderItems = await _unitOfWork.OrderItems.GetAllAsync();
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
                    return Forbid();
                }

                await _unitOfWork.BeginTransactionAsync();
                try
                {


                    // Perform all operations
                    await _unitOfWork.OrderItems.AddOrderItemAsync(createOrderItemDto);

                    // Save changes
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();

                    // Map and return
                    var orderItem = _mapper.Map<OrderItem>(createOrderItemDto);
                    var orderItemDto = _mapper.Map<OrderItemDto>(orderItem);

                    return CreatedAtAction(nameof(GetOrderItems), new { id = orderItem.Id }, orderItemDto);
                }
                catch
                {
                    // Rollback on error
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }


                }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
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
                
                await _unitOfWork.BeginTransactionAsync();

                try { 

                var orderItem = await _unitOfWork.OrderItems.GetByIdAsync(id);

                if (orderItem == null)
                {
                    return NotFound($"Order item with id {id} not found");
                }

                // Ensure user can only delete items from their own order
                if (orderItem.Order.UserId != currentUserId)
                {
                    return Forbid(); // or return Unauthorized();
                }

                await _unitOfWork.OrderItems.DeleteAsync(orderItem.Id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                    return NoContent();
                }
                catch
                {
                    // Rollback on error
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
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
