
using E_Commerce.DTOs.ProductDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProoductController
    {
        private readonly IProductRepository _productRepo;

        public ProoductController(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _productRepo.GetAllAsync();

                return new OkObjectResult(products);

            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while retrieving products: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }


        //get product by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                
                var product = await _productRepo.GetByIdAsync(id);
                if (product == null)
                {
                    return new NotFoundObjectResult($"Product with id {id} not found");
                }
                ProductDto productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    SellerId = product.SellerId,
                    CategoryId = product.CategoryId
                };
                return new OkObjectResult(productDto);
            }
            catch (KeyNotFoundException knfEx)
            {
                return new NotFoundObjectResult(knfEx.Message);
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while retrieving the product: {ex.Message}")
                {
                    StatusCode = 500
                };
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
                return new OkObjectResult("Product added successfully");
            }
            catch (ArgumentNullException anEx)
            {
                return new BadRequestObjectResult(anEx.Message);
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while adding the product: {ex.Message}")
                {
                    StatusCode = 500
                };
            }



        }

        [HttpPut]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
        {
            try
            {
               

                var product = await _productRepo.GetByIdAsync(id);

                if (product == null)
                {
                    return new NotFoundObjectResult($"Product with id {id} not found");
                }

                product.Stock = updateProductDto.Stock;
                product.Price = updateProductDto.Price;
                product.Description = updateProductDto.Description;
                product.Name = updateProductDto.Name;
                product.CategoryId = updateProductDto.CategoryId;



                await _productRepo.UpdateAsync(product);
                return new OkObjectResult("Product updated successfully");
            }
            catch (KeyNotFoundException knfEx)
            {
                return new NotFoundObjectResult(knfEx.Message);
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
                if (!await _productRepo.ExistsAsync(id))
                {
                    return new NotFoundObjectResult($"Product with id {id} not found");
                }

                await _productRepo.DeleteAsync(id);

                return new OkObjectResult("Product deleted successfully");
            }
            catch (KeyNotFoundException knfEx)
            {
                return new NotFoundObjectResult(knfEx.Message);
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
