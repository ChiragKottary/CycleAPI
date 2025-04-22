using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;

namespace CycleAPI.Repositories.Interface
{
    public interface IStockMovementRepository
    {
        Task<StockMovement?> GetByIdAsync(Guid id);
        Task<IEnumerable<StockMovement>> GetAllAsync();
        Task<IEnumerable<StockMovement>> GetByCycleIdAsync(Guid cycleId);
        Task<IEnumerable<StockMovement>> GetByUserIdAsync(Guid userId);
        Task<StockMovement> AddAsync(StockMovement stockMovement);
        Task<StockMovement> UpdateAsync(StockMovement stockMovement);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<(IEnumerable<StockMovement> Movements, int TotalCount)> GetFilteredAsync(StockMovementQueryParameters parameters);
        Task<int> GetCurrentStockLevelAsync(Guid cycleId);
        Task<bool> SaveChangesAsync();
    }
}