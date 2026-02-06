
using AutoMapper;
using E_Commerce.DTOs.CategoryDTO;
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
    public class CategoryController : ControllerBase
    {
 
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IUnitOfWork unitOfWork, IMapper mapper,ILogger<CategoryController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }


        //Get all categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                _logger.LogInformation($"Retrieved {categories.Count()} categories");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while retrieving categories: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //Get category by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving category with id {id}");
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                
                if (category == null)
                {
                    return NotFound($"Category with id {id} not found");
                }

                var categoryDto = _mapper.Map<Category>(category);
                _logger.LogInformation($"Category with id {id} retrieved successfully");

                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while retrieving the category: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //Add new category

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                //check validation on dto
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation($"Adding new category with name {createCategoryDto.Name}");
                    var category = _mapper.Map<Category>(createCategoryDto);
                    await _unitOfWork.Categories.AddAsync(category);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    _logger.LogInformation($"Category with name {createCategoryDto.Name} added successfully");
                    return StatusCode(201, new { message = "Category created successfully" });
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError($"An error occurred while adding category with name {createCategoryDto.Name}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while adding the category: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //Delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id) 
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation($"Deleting category with id {id}");
                    if (!await _unitOfWork.Categories.ExistsAsync(id))
                    {
                        return NotFound($"Category with id {id} not found");
                    }
                    await _unitOfWork.Categories.DeleteAsync(id);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation($"Category with id {id} deleted successfully");  
                    return Ok(new { message = "Category deleted successfully" });
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError($"An error occurred while deleting category with id {id}");
                    throw;

                }
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while deleting the category: {ex.Message}")
                {
                    StatusCode = 500
                };
            }


        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation($"Updating category with id {id}");
                    var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id);
                    if (existingCategory == null)
                    {
                        return NotFound($"Category with id {id} not found");
                    }
                    _mapper.Map(updateCategoryDto, existingCategory);
                    await _unitOfWork.Categories.UpdateAsync(existingCategory);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    _logger.LogInformation($"Category with id {id} updated successfully");

                    return Ok(new { message = "Category updated successfully" });
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError($"An error occurred while updating category with id {id}");
                    throw;
                }
            }
                catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while updating the category: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }






    }
}
