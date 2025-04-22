using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;

namespace CycleAPI.Repositories.Interface
{
    public interface ICycleTypeRepository
    {
        Task<CycleType?> GetByIdAsync(Guid id);
        Task<IEnumerable<CycleType>> GetAllAsync();
        Task<CycleType> AddAsync(CycleType cycleType);
        Task<CycleType> UpdateAsync(CycleType cycleType);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<(IEnumerable<CycleType> Types, int TotalCount)> GetFilteredAsync(CycleTypeQueryParameters parameters);
        Task<bool> SaveChangesAsync();
    }
}
