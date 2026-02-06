using E_Commerce.Data;
using E_Commerce.DTOs.ReviewDTO;
using E_Commerce.Models;
using E_Commerce.Repositrories.E_Commerce.Repositories;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Repositrories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public ReviewRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId)
        {
            //return all reviews for a product with each review's username by using the userId in both reviews and users table
            /* return await _dbContext.Reviews
                 .AsNoTracking()
                 .Where(r => r.ProductId == productId)
                 .Include(r => r.User)// Include the User navigation property
                 .Select(r=> new ReviewDto { 

                     ProductId = productId,
                     Rating = r.Rating,
                     Comment = r.Comment,
                     CreatedAt = r.CreatedAt,
                     UserId = r.UserId,
                     Username = r.User.UserName // Map the Username from the User navigation property
                 }

                 )
                 .ToListAsync();*/

            return await _dbContext.Reviews
                                          .AsNoTracking()
                                          .Where(r => r.ProductId == productId)
                                          .Include(r => r.User)   // Load related User
                                          .Select(r => new ReviewDto
                                          {
                                              ProductId = r.ProductId,
                                              Rating = r.Rating,
                                              Comment = r.Comment,
                                              CreatedAt = r.CreatedAt,
                                              UserId = r.UserId,
                                              Username = r.User.UserName
                                          })
                                          .ToListAsync();

        }
    }
}
