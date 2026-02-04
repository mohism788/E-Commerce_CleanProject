using AutoMapper;
using E_Commerce.DTOs.ReviewDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.Interfaces;
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
        private readonly IReviewRepository _reviewRepo;
        private readonly IMapper _mapper;

        public ReviewController(IReviewRepository reviewRepo, IMapper mapper)
        {
            _reviewRepo = reviewRepo;
            _mapper = mapper;
        }

        //Get reviews by product id
        [HttpGet("reviews/{productId}/reviews")]
        [Authorize]
        public async Task<IActionResult> GetReviewsByProductId(int productId)
        {
            try
            {
                var reviews = await _reviewRepo.GetReviewsByProductIdAsync(productId);
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
                var review = _mapper.Map<Review>(createReviewDto);
                review.CreatedAt = DateTime.UtcNow;
                await _reviewRepo.AddAsync(review);
                return CreatedAtAction(nameof(GetReviewsByProductId), new { productId = review.ProductId }, review);
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

                var existingReview = await _reviewRepo.GetByIdAsync(id);
                if (existingReview == null)
                {
                    return NotFound($"Review with id {id} not found");
                }
                if (existingReview.UserId != currentUserId)
                {
                    return Forbid(); // or return Unauthorized();
                }
                _mapper.Map(updateReviewDto, existingReview);
                await _reviewRepo.UpdateAsync(existingReview);
                return NoContent();
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
        [HttpDelete]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var existingReview = await _reviewRepo.GetByIdAsync(id);

                if (existingReview == null)
                {
                    return NotFound($"Review with id {id} not found");
                }
                if (existingReview.UserId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid(); // or return Unauthorized();
                }

                await _reviewRepo.DeleteAsync(existingReview.Id);
                return NoContent();
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
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID claim not found or invalid.");
        }

    }
}
