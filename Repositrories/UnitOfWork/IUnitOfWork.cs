using System.Data;
using E_Commerce.Data;
using E_Commerce.Repositrories.Interfaces;
using Microsoft.EntityFrameworkCore;

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
        public ApplicationDbContext GetDbContext();

        // Execution Strategy for manual transactions (required for EnableRetryOnFailure)
        Task ExecuteActionStrategyAsync(Func<Task> action);
        Task<T> ExecuteResultStrategyAsync<T>(Func<Task<T>> action);

        // Check if transaction is active
        bool HasActiveTransaction { get; }
    }
}
