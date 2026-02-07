
using System.Security.Claims;
using AutoMapper;
using E_Commerce.DTOs.OrderDTO;
using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Repositrories.UnitOfWork;
using E_Commerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderService _orderService;

        public OrderController(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<OrderController> logger,
            IOrderService orderService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _orderService = orderService;
        }

        // GET: api/orders
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetAllOrders()
        {
            try
            {
                _logger.LogInformation("Retrieving all orders");
                var orders = await _unitOfWork.Orders.GetAllAsync();
                var orderDtos = _mapper.Map<List<OrderDto>>(orders);

                _logger.LogInformation($"Retrieved {orderDtos.Count} orders successfully");
                return Ok(new ApiResponse<List<OrderDto>>
                {
                    Success = true,
                    Data = orderDtos,
                    Message = "Orders retrieved successfully",
                    Count = orderDtos.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all orders");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiErrorResponse
                    {
                        Success = false,
                        Message = "An error occurred while retrieving orders",
                        StatusCode = StatusCodes.Status500InternalServerError
                    });
            }
        }

        // GET: api/orders/{orderId}
        [HttpGet("{orderId:int}")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderById(int orderId)
        {
            try
            {
                var currentUserId = GetUserIdFromToken();
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Success = false,
                        Message = $"Order with id {orderId} not found",
                        StatusCode = StatusCodes.Status404NotFound
                    });
                }
                
                if (order.UserId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid(); 
                }

                var orderDto = _mapper.Map<OrderDto>(order);

                return Ok(new ApiResponse<OrderDto>
                {
                    Success = true,
                    Data = orderDto,
                    Message = "Order retrieved successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponse
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with ID {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiErrorResponse
                    {
                        Success = false,
                        Message = "An error occurred while retrieving the order",
                        StatusCode = StatusCodes.Status500InternalServerError
                    });
            }
        }

        // GET: api/orders/user/{userId}/orders
        [HttpGet("user/{userId}/orders")]
        [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetOrdersByUserId(Guid userId)
        {
            try
            {
                var currentUserId = GetUserIdFromToken();
                
                       
                 var orders = await _unitOfWork.Orders.GetUserOrdersAsync(userId);

                // Ensure user can only access their own orders unless they are an Admin
                if (userId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var orderDtos = _mapper.Map<List<OrderDto>>(orders);


                return Ok(new ApiResponse<List<OrderDto>>
                {
                    Success = true,
                    Data = orderDtos,
                    Message = "User orders retrieved successfully",
                    Count = orderDtos.Count
                });
                      
                      
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponse
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for user {userId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiErrorResponse
                    {
                        Success = false,
                        Message = "An error occurred while retrieving user orders",
                        StatusCode = StatusCodes.Status500InternalServerError
                    });
            }
        }

        // POST: api/orders/buy-now
        [HttpPost("buy-now")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> BuyNow([FromBody] BuyNowOrderItemDto buyNowDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList(),
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                // Get userId from authentication token (not from parameter)
                var userId = GetUserIdFromToken();
               
                 var order = await _orderService.BuyNowAsync(userId, buyNowDto);
                 
                 var orderDto = _mapper.Map<OrderDto>(order);


                 return CreatedAtAction(nameof(GetOrderById),
                     new { orderId = order.Id },
                     new ApiResponse<OrderDto>
                     {
                         Success = true,
                         Data = orderDto,
                         Message = "Order created successfully via Buy Now"
                     });
                        
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = StatusCodes.Status404NotFound
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Buy Now order");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiErrorResponse
                    {
                        Success = false,
                        Message = "An error occurred while processing the order",
                        StatusCode = StatusCodes.Status500InternalServerError
                    });
            }
        }




        //delete order by id
        [HttpDelete("{orderId:int}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> DeleteOrder(int orderId)
        {
            try
            {
                var currentUserId = GetUserIdFromToken();
                await _unitOfWork.BeginTransactionAsync();
                try { 
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Success = false,
                        Message = $"Order with id {orderId} not found",
                        StatusCode = StatusCodes.Status404NotFound
                    });
                }

                if (order.UserId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }
                await _unitOfWork.Orders.DeleteAsync(order.Id);
                await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = null,
                    Message = "Order deleted successfully"
                });
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiErrorResponse
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order with ID {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiErrorResponse
                    {
                        Success = false,
                        Message = "An error occurred while deleting the order",
                        StatusCode = StatusCodes.Status500InternalServerError
                    });
            }
        }

        // POST: api/orders/checkout
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> Checkout()
        {
            try
            {
                // Get userId from authentication token
                var userId = GetUserIdFromToken();

                // Log the checkout attempt
                _logger.LogInformation("Checkout initiated for user {UserId}", userId);

                // Process checkout using the service
                var order = await _orderService.CheckoutAsync(userId);

                // Map to DTO
                var orderDto = _mapper.Map<OrderDto>(order);

                // Log successful checkout
                _logger.LogInformation("Checkout completed successfully for user {UserId}, Order ID: {OrderId}",
                    userId, order.Id);

                return Ok(new ApiResponse<OrderDto>
                {
                    Success = true,
                    Data = orderDto,
                    Message = "Checkout completed successfully"
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Cart is empty"))
            {
                _logger.LogWarning("Checkout failed: Cart is empty for user {UserId}", GetUserIdFromToken());

                return BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = "Cannot checkout with an empty cart",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient stock"))
            {
                _logger.LogWarning("Checkout failed: {Message}", ex.Message);

                return BadRequest(new ApiErrorResponse
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Checkout failed: Resource not found");

                return NotFound(new ApiErrorResponse
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = StatusCodes.Status404NotFound
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Checkout failed: Authorization error");

                return Unauthorized(new ApiErrorResponse
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout for user {UserId}", GetUserIdFromToken());

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiErrorResponse
                    {
                        Success = false,
                        Message = "An error occurred during checkout. Please try again.",
                        StatusCode = StatusCodes.Status500InternalServerError
                    });
            }
        }



        // ========== Response Models ==========

        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
            public int? Count { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }

        public class ApiErrorResponse
        {
            public bool Success { get; set; } = false;
            public string Message { get; set; } = string.Empty;
            public List<string>? Errors { get; set; }
            public int StatusCode { get; set; }
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }

        // ========== Helper Methods ==========

        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId" ||
                                                           c.Type == "sub" ||
                                                           c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim is null)
            {
                throw new UnauthorizedAccessException("User ID claim is missing.");
            }

            if (!Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User ID claim is invalid.");
            }

            return userId;
        }

       

    }
}