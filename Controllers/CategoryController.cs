
using AutoMapper;
using E_Commerce.DTOs.CategoryDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryRepository categoryRepo, IMapper mapper)
        {
            _categoryRepo = categoryRepo;
            _mapper = mapper;
        }


        //Get all categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _categoryRepo.GetAllAsync();
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
                var category = await _categoryRepo.GetByIdAsync(id);
                
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

                var category = _mapper.Map<Category>(createCategoryDto);
                await _categoryRepo.AddAsync(category);
                return StatusCode(201, new { message = "Category created successfully" });
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
                if (!await _categoryRepo.ExistsAsync(id))
                {
                    return NotFound($"Category with id {id} not found");
                }
                await _categoryRepo.DeleteAsync(id);
                return Ok(new { message = "Category deleted successfully" });
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
                var existingCategory = await _categoryRepo.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound($"Category with id {id} not found");
                }
                _mapper.Map(updateCategoryDto, existingCategory);
                await _categoryRepo.UpdateAsync(existingCategory);
                return Ok(new { message = "Category updated successfully" });
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
