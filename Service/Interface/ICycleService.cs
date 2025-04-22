using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Service.Interface
{
    public interface ICycleService
    {
        Task<Cycle?> GetCycleByIdAsync(Guid id);
        Task<IEnumerable<Cycle>> GetAllCyclesAsync();
        Task<Cycle> CreateCycleAsync(Cycle cycle);
        Task<Cycle?> UpdateCycleAsync(Cycle cycle);
        Task<bool> DeleteCycleAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<PagedResult<Cycle>> GetFilteredCyclesAsync(CycleQueryParameters parameters);
    }
}