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
        private readonly ILogger<OrderItemController> _logger;

        public OrderItemController(IUnitOfWork unitOfWork, IMapper mapper,ILogger<OrderItemController> logger)
        {

            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
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
                _logger.LogInformation($"Retrieved {orderItemDtos.Count()} order items");


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
        public async Task<IActionResult> CreateOrderItem([FromBody] CreateOrderItemDto createOrderItemDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();


                await _unitOfWork.BeginTransactionAsync();
                try
                {

                    _logger.LogInformation($"Creating order item for user {currentUserId} with product {createOrderItemDto.ProductId} and quantity {createOrderItemDto.Quantity}");
                    // Perform all operations
                    await _unitOfWork.OrderItems.AddOrderItemAsync(createOrderItemDto);

                    // Save changes
                    await _unitOfWork.SaveChangesAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();

                    // Map and return
                    var orderItem = _mapper.Map<OrderItem>(createOrderItemDto);
                    var orderItemDto = _mapper.Map<OrderItemDto>(orderItem);
                    _logger.LogInformation($"Order item created successfully for user {currentUserId} with product {createOrderItemDto.ProductId} and quantity {createOrderItemDto.Quantity}");

                    return CreatedAtAction(nameof(GetOrderItems), new { id = orderItem.Id }, orderItemDto);
                }
                catch
                {
                    // Rollback on error
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError($"Error occurred while creating order item for user {currentUserId} with product {createOrderItemDto.ProductId} and quantity {createOrderItemDto.Quantity}. Transaction rolled back.");
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
                    _logger.LogInformation($"Attempting to delete order item with id {id} for user {currentUserId}");

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
                    _logger.LogInformation($"Order item with id {id} deleted successfully for user {currentUserId}");
                    return NoContent();
                }
                catch
                {
                    // Rollback on error
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError($"An error occurred while deleting order item with id {id} for user {currentUserId}. Transaction rolled back.");
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
