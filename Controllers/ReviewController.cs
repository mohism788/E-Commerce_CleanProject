using System.Security.Claims;
using AutoMapper;
using E_Commerce.DTOs.ReviewDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
using E_Commerce.Repositrories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IUnitOfWork unitOfWork, IMapper mapper,ILogger<ReviewController> logger)
        {
            
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        //Get reviews by product id
        [HttpGet("reviews/{productId}/reviews")]
        public async Task<IActionResult> GetReviewsByProductId(int productId)
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetReviewsByProductIdAsync(productId);
                _logger.LogInformation($"Retrieved {reviews.Count()} reviews for product with id {productId}");
                return Ok(reviews);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while retrieving reviews: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //create new review 
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateReview ([FromBody] CreateReviewDto createReviewDto)
        {
            try
            {
                return await _unitOfWork.ExecuteResultStrategyAsync<IActionResult>(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        var currentUserId = GetCurrentUserId();
                        _logger.LogInformation($"Creating review for product with id {createReviewDto.ProductId} by user with id {currentUserId}");
                        var review = _mapper.Map<Review>(createReviewDto);
                        review.UserId = currentUserId; // Force identity from token
                        review.CreatedAt = DateTime.UtcNow;
                        await _unitOfWork.Reviews.AddAsync(review);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        _logger.LogInformation($"Review created successfully for product with id {review.ProductId} by user with id {currentUserId}");
                        return CreatedAtAction(nameof(GetReviewsByProductId), new { productId = review.ProductId }, review);
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError($"An error occurred while creating review for product with id {createReviewDto.ProductId} by user with id {GetCurrentUserId()}");
                        throw;
                    }
                });
            }

            catch (Exception ex)
            {
                return new ObjectResult($"An error occurred while creating the review: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles="Customer")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                return await _unitOfWork.ExecuteResultStrategyAsync<IActionResult>(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        _logger.LogInformation($"Updating review with id {id} by user with id {currentUserId}");
                        var existingReview = await _unitOfWork.Reviews.GetByIdAsync(id);
                        if (existingReview == null)
                        {
                            return NotFound($"Review with id {id} not found");
                        }
                        if (existingReview.UserId != currentUserId)
                        {
                            return Forbid(); // or return Unauthorized();
                        }
                        _mapper.Map(updateReviewDto, existingReview);
                        await _unitOfWork.Reviews.UpdateAsync(existingReview);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        _logger.LogInformation($"Review with id {id} updated successfully by user with id {currentUserId}");
                        return NoContent();
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError($"An error occurred while updating review with id {id} by user with id {currentUserId}");
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
                return new ObjectResult($"An error occurred while updating the review: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }

        //delete certain review
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                return await _unitOfWork.ExecuteResultStrategyAsync<IActionResult>(async () =>
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        _logger.LogInformation($"Deleting review with id {id} by user with id {currentUserId}");
                        var existingReview = await _unitOfWork.Reviews.GetByIdAsync(id);

                        if (existingReview == null)
                        {
                            return NotFound($"Review with id {id} not found");
                        }
                        if (existingReview.UserId != currentUserId && !User.IsInRole("Admin"))
                        {
                            return Forbid(); // or return Unauthorized();
                        }

                        await _unitOfWork.Reviews.DeleteAsync(existingReview.Id);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        _logger.LogInformation($"Review with id {id} deleted successfully by user with id {currentUserId}");
                        return NoContent();
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError($"An error occurred while deleting review with id {id} by user with id {currentUserId}");
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
                return new ObjectResult($"An error occurred while deleting the review: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }


        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return userId;
        }

    }
}
