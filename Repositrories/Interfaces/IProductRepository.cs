using E_Commerce.Models;

namespace E_Commerce.Repositrories.Interfaces
{
    public interface IProductRepository
    {
        // Define method signatures for product-related data operations 

        //get products
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);

        //get seller Id by product id
        Task<Guid> GetSellerIdByProductIdAsync(int productId);




    }
}
