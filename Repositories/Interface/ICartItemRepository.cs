using CycleAPI.Models.Domain;

namespace CycleAPI.Repositories.Interface
{
    public interface ICartItemRepository
    {
        Task<CartItem?> GetByIdAsync(Guid cartItemId);
        Task<IEnumerable<CartItem>> GetAllAsync(Guid cartId);
        Task<CartItem?> GetByCartAndCycleAsync(Guid cartId, Guid cycleId);
        Task<CartItem> AddAsync(Guid cartId, Guid cycleId, int quantity);
        Task<CartItem?> UpdateAsync(Guid cartItemId, int quantity);
        Task<bool> DeleteAsync(Guid cartItemId);
        Task<bool> ExistsAsync(Guid cartItemId);
        Task<bool> SaveChangesAsync();
    }
}
