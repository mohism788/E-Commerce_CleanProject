// Data/UnitOfWork.cs
using E_Commerce.Repositrories.E_Commerce.Repositories;
using E_Commerce.Repositrories;
using E_Commerce.Repositrories.Interfaces;
using E_Commerce.Repositrories.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using E_Commerce.Exceptions;
using E_Commerce.Models;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Data
{
    public class UnitOfWorkClass : IUnitOfWork
    {
        private readonly ApplicationDbContext _context; 
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<UnitOfWorkClass> _logger;
        private IDbContextTransaction _currentTransaction;
        private bool _disposed = false;

        // Cache for repositories
        private Dictionary<Type, object> _repositories;

        public UnitOfWorkClass(ApplicationDbContext context, ILoggerFactory loggerFactory,ILogger<UnitOfWorkClass> logger)
        {
            _context = context;
            _loggerFactory = loggerFactory;
            _logger = logger;
            _repositories = new Dictionary<Type, object>();
        }

        // ========== Repository Properties ==========
        private IOrderRepository _orders;
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);

        private IProductRepository _products;
        public IProductRepository Products => _products ??= new ProductRepository(_context);

        private ICategoryRepository _categories;
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

        private ICartItemRepository _cartItems;
        public ICartItemRepository CartItems => _cartItems ??= new CartItemRepository(_context, _loggerFactory.CreateLogger<CartItemRepository>());

        private IOrderItemRepository _orderItems;
        public IOrderItemRepository OrderItems => _orderItems ??= new OrderItemRepository(_context);

        private IReviewRepository _reviews;
        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);

        

        // ========== Generic Repository Access ==========
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new GenericRepository<T>(_context);
            }
            _logger.LogInformation("Accessing repository for type {Type}", type.Name);
            return (IGenericRepository<T>)_repositories[type];
        }

        // ========== Save Changes ==========
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
               _logger.LogInformation("Saving changes to database");
                return await _context.SaveChangesAsync(cancellationToken);
               
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while saving changes to the database");
                throw new UnitOfWorkException("Failed to save changes to database", ex);
            }
        }

        //private void SetAuditFields()
        //{
        //    var entries = _context.ChangeTracker.Entries()
        //        .Where(e => e.Entity is BaseEntity &&
        //            (e.State == EntityState.Added || e.State == EntityState.Modified));

        //    foreach (var entry in entries)
        //    {
        //        var entity = (BaseEntity)entry.Entity;

        //        if (entry.State == EntityState.Added)
        //        {
        //            entity.CreatedAt = DateTime.UtcNow;
        //            entity.CreatedBy = GetCurrentUserId();
        //        }

        //        entity.UpdatedAt = DateTime.UtcNow;
        //        entity.UpdatedBy = GetCurrentUserId();
        //    }
        //}

        private string GetCurrentUserId()
        {
            // Implement your logic to get current user ID
            // Could be from HttpContext, ClaimsPrincipal, etc.
            return "system"; // Default for now
        }

        // ========== Transaction Management ==========
        public bool HasActiveTransaction => _currentTransaction != null;

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_currentTransaction != null)
                throw new UnitOfWorkException("A transaction is already in progress");

            _currentTransaction = await _context.Database.BeginTransactionAsync(isolationLevel);
            _logger.LogInformation("Transaction started with isolation level {IsolationLevel}", isolationLevel);
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null)
                throw new UnitOfWorkException("No active transaction to commit");

            try
            {
                await SaveChangesAsync();
                await _currentTransaction.CommitAsync();
                _logger.LogInformation("Transaction committed successfully");
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                DisposeTransaction();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction == null)
                throw new UnitOfWorkException("No active transaction to rollback");

            try
            {
                await _currentTransaction.RollbackAsync();
                _logger.LogInformation("Transaction rolled back");
            }
            finally
            {
                DisposeTransaction();
            }
        }

        private void DisposeTransaction()
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }

        public ApplicationDbContext GetDbContext() => _context; // Added for repository access


        // ========== IDisposable Implementation ==========
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeTransaction();
                    _context?.Dispose();
                }

                _disposed = true;
            }
        }

        // ========== Helper Methods ==========
        public async Task ExecuteInTransactionAsync(Func<Task> action,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            await BeginTransactionAsync(isolationLevel);

            try
            {
                await action();
                await CommitTransactionAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            await BeginTransactionAsync(isolationLevel);

            try
            {
                var result = await action();
                await CommitTransactionAsync();
                return result;
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }
    }
}