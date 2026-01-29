using AutoMapper;
using E_Commerce.DTOs.OrderDTO;
using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepo;

        public OrderController(IMapper mapper, IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
            _mapper = mapper;
        }

        //get all orders

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderRepo.GetAllAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while retrieving orders: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //checkout order
        [HttpPost("checkout")]
        public async Task<IActionResult> CheckoutOrder(Guid userId)
        {
            try
            {
                var order = await _orderRepo.CheckoutAsync(userId);
                return Ok("Order created successfully!");
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while checking out the order: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //get orders by user id
        [HttpGet("user/{userId}/orders")]
        public async Task<IActionResult> GetOrdersByUserId(Guid userId)
        {
            try
            {
                var orders = await _orderRepo.GetUserOrdersAsync(userId);
                var orderDtos = _mapper.Map<List<OrderDto>>(orders);



                return Ok(orderDtos);
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while retrieving orders: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //get order by id
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                var order = await _orderRepo.GetByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound($"Order with id {orderId} not found");
                }
                var orderDto = _mapper.Map<OrderDto>(order);
               
                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while retrieving the order: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        
        [HttpPost("buy-now")]
        public async Task<IActionResult> BuyNow([FromBody] BuyNowOrderItemDto buyNowDto, Guid userId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }
                var order = await _orderRepo.BuyNowAsync(userId, buyNowDto);
                return Ok("Order created successfully!");
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while processing the buy now order: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }
    }
}
