using System.Security.Claims;
using AutoMapper;
using E_Commerce.DTOs.ProductDTO;
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
    public class ProductController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParameters queryParameters)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                try { 
                var pagedResult = await _unitOfWork.Products.GetProductsAsync(queryParameters);

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
                catch
                {
                    // Rollback on error
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { Success = false, Message = $"Product with id {id} not found" });
                }
                return Ok(new { Success = true, Data = product });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        //add product
        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> AddProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                var sellerId = GetCurrentUserId();



                Product product = new Product
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    Stock = createProductDto.Stock,
                    SellerId = sellerId,
                    CategoryId = createProductDto.CategoryId
                };

                await _unitOfWork.BeginTransactionAsync();
                try { 
                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                    return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
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

                await _unitOfWork.BeginTransactionAsync();
                try { 
                var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
                
                if (existingProduct == null)
                {
                    return NotFound($"Product with id {id} not found");
                }
                if (existingProduct.SellerId != currentUserId)
                {
                    return Forbid(); // or return Unauthorized();
                }
                _mapper.Map(updateProductDto, existingProduct);
                await _unitOfWork.Products.UpdateAsync(existingProduct);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                    return Ok(new { message = "Product updated successfully" });
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
                await _unitOfWork.BeginTransactionAsync();
                try { 
                var product = await _unitOfWork.Products.GetByIdAsync(id);


                if (product == null)
                {
                    return NotFound($"Product with id {id} not found");
                }
                if (product.SellerId != currentUserId)
                {
                    return Forbid(); // or return Unauthorized();
                }

                await _unitOfWork.Products.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                    return Ok(new { message = "Product deleted successfully" });
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
                return new ObjectResult($"An error occurred while deleting the product: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //get user by token
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

