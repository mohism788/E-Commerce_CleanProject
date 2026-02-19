using System.Security.Claims;
using AutoMapper;
using E_Commerce.DTOs.ProductDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
using E_Commerce.Repositrories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    public class ProductController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParameters queryParameters, CancellationToken cancellationToken)
        {
            try
            {
                

                var pagedResult = await _unitOfWork.Products.GetProductsAsync(queryParameters);
                _logger.LogInformation($"Retrieved {pagedResult.Items.Count()} products (Page {pagedResult.Page} of {pagedResult.TotalPages}) with filters: Name={queryParameters.Name}, CategoryId={queryParameters.CategoryId}, MinPrice={queryParameters.MinPrice}, MaxPrice={queryParameters.MaxPrice}");
                var products = _mapper.Map<IEnumerable<ProductDto>>(pagedResult.Items);

                // Fetch seller names for all products on this page efficiently
                var sellerIds = products.Select(p => p.SellerId).Distinct().ToList();
                var sellers = await _unitOfWork.GetDbContext().Users
                    .AsNoTracking()
                    .Where(u => sellerIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.UserName, cancellationToken);

                foreach (var productDto in products)
                {
                    if (sellers.TryGetValue(productDto.SellerId, out var userName))
                    {
                        productDto.SellerName = userName;
                    }
                    else
                    {
                        productDto.SellerName = "Verified Seller";
                    }
                }

                return Ok(new
                {
                    Success = true,
                    Data = products,
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Retrieving product with id {id}");
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { Success = false, Message = $"Product with id {id} not found" });
                }
                var productDto = _mapper.Map<ProductDto>(product);
                
                // Fetch seller name
                var seller = await _unitOfWork.GetDbContext().Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == product.SellerId, cancellationToken);
                productDto.SellerName = seller?.UserName ?? "Verified Seller";

                _logger.LogInformation($"Product with id {id} retrieved successfully: {product.Name}");
                return Ok(new { Success = true, Data = productDto });
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

                return await _unitOfWork.ExecuteResultStrategyAsync<IActionResult>(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    { 
                        _logger.LogInformation($"Adding new product '{product.Name}' for seller {sellerId}");
                        await _unitOfWork.Products.AddAsync(product);
                           await _unitOfWork.SaveChangesAsync();
                           await _unitOfWork.CommitTransactionAsync();

                            _logger.LogInformation($"Product '{product.Name}' added successfully with id {product.Id}");
                        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
                    }
                    catch
                    {
                        // Rollback on error
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError($"Error occurred while adding product '{product.Name}' for seller {sellerId}. Transaction rolled back.");
                        throw;
                    }
                });
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

                return await _unitOfWork.ExecuteResultStrategyAsync<IActionResult>(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        _logger.LogInformation($"Updating product with id {id} for seller {currentUserId}");
                        var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);

                        if (existingProduct == null)
                        {
                            return NotFound($"Product with id {id} not found");
                        }
                        if (existingProduct.SellerId != currentUserId)
                        {
                            return Forbid();
                        }
                        _mapper.Map(updateProductDto, existingProduct);
                        await _unitOfWork.Products.UpdateAsync(existingProduct);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        _logger.LogInformation($"Product with id {id} updated successfully for seller {currentUserId}");
                        return Ok(new { message = "Product updated successfully" });
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError($"Error occurred while updating product with id {id} for seller {currentUserId}. Transaction rolled back.");
                        throw;
                    }
                });
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
                return await _unitOfWork.ExecuteResultStrategyAsync<IActionResult>(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        _logger.LogInformation($"Deleting product with id {id} for seller {currentUserId}");
                        var product = await _unitOfWork.Products.GetByIdAsync(id);

                        if (product == null)
                        {
                            return NotFound($"Product with id {id} not found");
                        }
                        if (product.SellerId != currentUserId)
                        {
                            return Forbid();
                        }

                        await _unitOfWork.Products.DeleteAsync(id);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        _logger.LogInformation($"Product with id {id} deleted successfully for seller {currentUserId}");
                        return Ok(new { message = "Product deleted successfully" });
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw;
                    }
                });
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
    }
    
}
