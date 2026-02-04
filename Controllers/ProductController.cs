using AutoMapper;
using E_Commerce.DTOs.ProductDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepo;
        private readonly IMapper _mapper;

        public ProductController(IProductRepository productRepo, IMapper mapper)
        {
            _productRepo = productRepo;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParameters queryParameters)
        {
            try
            {
                var pagedResult = await _productRepo.GetProductsAsync(queryParameters);

                return Ok(new
                {
                    Success = true,
                    Data = pagedResult.Items,
                    Pagination = new
                    {
                        pagedResult.Page,
                        pagedResult.PageSize,
                        pagedResult.TotalCount,
                        pagedResult.TotalPages,
                        pagedResult.HasPreviousPage,
                        pagedResult.HasNextPage
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }


        //add product
        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> AddProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                
                Product product = new Product
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    Stock = createProductDto.Stock,
                    SellerId = createProductDto.SellerId,
                    CategoryId = createProductDto.CategoryId
                };
                await _productRepo.AddAsync(product);
                return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while adding the product: {ex.Message}")
                {
                    StatusCode = 500
                };
            }

        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var existingProduct = await _productRepo.GetByIdAsync(id);
                
                if (existingProduct == null)
                {
                    return NotFound($"Product with id {id} not found");
                }
                if (existingProduct.SellerId != currentUserId)
                {
                    return Forbid(); // or return Unauthorized();
                }
                _mapper.Map(updateProductDto, existingProduct);
                await _productRepo.UpdateAsync(existingProduct);
                return Ok(new { message = "Product updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while updating the product: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var product = await _productRepo.GetByIdAsync(id);


                if (product == null)
                {
                    return NotFound($"Product with id {id} not found");
                }
                if (product.SellerId != currentUserId)
                {
                    return Forbid(); // or return Unauthorized();
                }

                await _productRepo.DeleteAsync(id);
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while deleting the product: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //get user by token
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID claim not found");
            }
            return Guid.Parse(userIdClaim.Value);
        }
    }
}

