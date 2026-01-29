using System.Data;
using E_Commerce.Repositrories.Interfaces;

namespace E_Commerce.Repositrories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {// Repositories
        IOrderRepository Orders { get; }
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        ICartItemRepository CartItems { get; }
        IOrderItemRepository OrderItems { get; }
        IReviewRepository Reviews { get; }

        // Generic repository access
        IGenericRepository<T> Repository<T>() where T : class;

        // Save changes
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Transaction management
        Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        // Check if transaction is active
        bool HasActiveTransaction { get; }
    }
}
