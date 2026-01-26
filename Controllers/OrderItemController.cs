using AutoMapper;
using E_Commerce.DTOs.OrderItemDTO;
using E_Commerce.DTOs.ReviewDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
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
        public async Task<IActionResult> CreateOrderItem([FromBody] CreateOrderItemDto createOrderItemDto, Guid userId)
        {
            try
            {
               
                await _orderItemRepo.AddOrderItemAsync(createOrderItemDto);

                var orderItem = _mapper.Map<OrderItem>(createOrderItemDto);
                var orderItemDto = _mapper.Map<OrderItemDto>(orderItem);
                return CreatedAtAction(nameof(GetOrderItems), new { id = orderItem.Id }, orderItemDto);
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
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            try
            {
                var orderItem = await _orderItemRepo.GetByIdAsync(id);
                if (orderItem == null)
                {
                    return NotFound($"Order item with id {id} not found");
                }
                await _orderItemRepo.DeleteAsync(orderItem.Id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while deleting the order item: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }



    }
}
