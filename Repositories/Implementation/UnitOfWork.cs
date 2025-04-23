using CycleAPI.Data;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CycleAPI.Repositories.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed;
        private IPaymentRepository _payments;

        public ICartRepository Carts { get; }
        public ICartItemRepository CartItems { get; }
        public ICycleRepository Cycles { get; }
        public ICustomerRepository Customers { get; }
        public ICartActivityLogRepository CartActivityLogs { get; }
        public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);

        public UnitOfWork(
            ApplicationDbContext context,
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            ICycleRepository cycleRepository,
            ICustomerRepository customerRepository,
            ICartActivityLogRepository cartActivityLogRepository)
        {
            _context = context;
            Carts = cartRepository;
            CartItems = cartItemRepository;
            Cycles = cycleRepository;
            Customers = customerRepository;
            CartActivityLogs = cartActivityLogRepository;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction?.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            await _transaction?.RollbackAsync();
            _transaction?.Dispose();
            _transaction = null;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}