
using E_Commerce.DTOs.ProductDTO;
using E_Commerce.Models;

namespace E_Commerce.Repositrories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {

        Task<PagedResult<Product>> GetProductsAsync(ProductQueryParameters queryParameters);

        Task<Guid> GetSellerIdByProductIdAsync(int productId);

        //Get all products for seller by sellerId
        Task<IEnumerable<Product>> GetProductsBySellerIdAsync(Guid sellerId);


       
















    }
}
