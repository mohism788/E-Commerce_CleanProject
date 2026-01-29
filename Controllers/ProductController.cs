using AutoMapper;
using E_Commerce.DTOs.ProductDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
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
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
        {
            try
            {
                var existingProduct = await _productRepo.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound($"Product with id {id} not found");
                }
                _mapper.Map(updateProductDto, existingProduct);
                await _productRepo.UpdateAsync(existingProduct);
                return Ok(new { message = "Product updated successfully" });
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
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                 
                if (await _productRepo.GetByIdAsync(id) == null)
                {
                    return NotFound($"Product with id {id} not found");
                }
                await _productRepo.DeleteAsync(id);
                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while deleting the product: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }
    }
}

