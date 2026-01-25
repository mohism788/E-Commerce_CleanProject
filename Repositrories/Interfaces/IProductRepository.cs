using E_Commerce.Models;

namespace E_Commerce.Repositrories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        
        Task<Guid> GetSellerIdByProductIdAsync(int productId);

        //Get all products for seller by sellerId
        Task<IEnumerable<Product>> GetProductsBySellerIdAsync(Guid sellerId);


        //get all products in a category id 
        Task<IEnumerable<Product>> GetAllProductInCategoryAsync(int categoryId);
















    }
}
