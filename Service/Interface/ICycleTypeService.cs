using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Service.Interface
{
    public interface ICycleTypeService
    {
        Task<CycleType?> GetByIdAsync(Guid id);
        Task<IEnumerable<CycleType>> GetAllAsync();
        Task<CycleType> CreateAsync(CycleType cycleType);
        Task<CycleType?> UpdateAsync(CycleType cycleType);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<PagedResult<CycleType>> GetFilteredTypesAsync(CycleTypeQueryParameters parameters);
    }
}