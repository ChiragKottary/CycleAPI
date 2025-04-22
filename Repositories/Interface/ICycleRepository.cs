using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;

namespace CycleAPI.Repositories.Interface
{
    public interface ICycleRepository
    {
        Task<Cycle?> GetByIdAsync(Guid id);
        Task<IEnumerable<Cycle>> GetAllAsync();
        Task<IEnumerable<Cycle>> GetByBrandIdAsync(Guid brandId);
        Task<IEnumerable<Cycle>> GetByTypeIdAsync(Guid typeId);
        Task<Cycle> AddAsync(Cycle cycle);
        Task<Cycle> UpdateAsync(Cycle cycle);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> UpdateStockAsync(Guid id, int quantity);
        Task<(IEnumerable<Cycle> Cycles, int TotalCount)> GetFilteredAsync(CycleQueryParameters parameters);
        Task<bool> SaveChangesAsync();
    }
}
