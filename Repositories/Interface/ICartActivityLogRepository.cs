using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;

namespace CycleAPI.Repositories.Interface
{
    public interface ICartActivityLogRepository
    {
        Task<CartActivityLog> AddAsync(CartActivityLog log);
        Task<IEnumerable<CartActivityLog>> GetByCartIdAsync(Guid cartId);
        Task<IEnumerable<CartActivityLog>> GetByCustomerIdAsync(Guid customerId);
        Task<(IEnumerable<CartActivityLog> Logs, int TotalCount)> GetFilteredAsync(CartActivityLogQueryParameters parameters);
        Task<bool> SaveChangesAsync();
    }
}