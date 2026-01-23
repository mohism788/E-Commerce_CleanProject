using E_Commerce.DTOs.ReviewDTO;
using E_Commerce.Models;

namespace E_Commerce.Repositrories.Interfaces
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        //get review by product id
        Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(int productId);


    }
}
