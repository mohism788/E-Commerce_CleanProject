
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

        public CategoryController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        //Get all categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
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
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                
                if (category == null)
                {
                    return NotFound($"Category with id {id} not found");
                }

                var categoryDto = _mapper.Map<Category>(category);
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

                    var category = _mapper.Map<Category>(createCategoryDto);
                    await _unitOfWork.Categories.AddAsync(category);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    return StatusCode(201, new { message = "Category created successfully" });
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
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
                    if (!await _unitOfWork.Categories.ExistsAsync(id))
                    {
                        return NotFound($"Category with id {id} not found");
                    }
                    await _unitOfWork.Categories.DeleteAsync(id);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    return Ok(new { message = "Category deleted successfully" });
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
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
                    var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id);
                    if (existingCategory == null)
                    {
                        return NotFound($"Category with id {id} not found");
                    }
                    _mapper.Map(updateCategoryDto, existingCategory);
                    await _unitOfWork.Categories.UpdateAsync(existingCategory);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Ok(new { message = "Category updated successfully" });
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
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
