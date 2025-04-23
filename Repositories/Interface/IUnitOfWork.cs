using System;
using System.Threading.Tasks;

namespace CycleAPI.Repositories.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        ICartRepository Carts { get; }
        ICartItemRepository CartItems { get; }
        ICycleRepository Cycles { get; }
        ICustomerRepository Customers { get; }
        ICartActivityLogRepository CartActivityLogs { get; }
        IPaymentRepository Payments { get; }
        Task<bool> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}