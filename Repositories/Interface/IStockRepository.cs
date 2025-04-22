using CycleAPI.Models.Domain;
using CycleAPI.Models.Enums;

namespace CycleAPI.Repositories.Interface
{
    public interface IStockRepository
    {
        Task<StockMovement> AddMovementAsync(StockMovement stockMovement);
        Task<IEnumerable<StockMovement>> GetByCycleIdAsync(Guid cycleId);
        Task<IEnumerable<StockMovement>> GetByTypeAsync(MovementType movementType);
        Task<bool> SaveChangesAsync();
    }
}
