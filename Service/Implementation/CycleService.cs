using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using CycleAPI.Service.Interface;

namespace CycleAPI.Service.Implementation
{
    public class CycleService : ICycleService
    {
        private readonly ICycleRepository _cycleRepository;

        public CycleService(ICycleRepository cycleRepository)
        {
            _cycleRepository = cycleRepository;
        }

        // ...existing code...

        public async Task<PagedResult<Cycle>> GetFilteredCyclesAsync(CycleQueryParameters parameters)
        {
            var (cycles, totalCount) = await _cycleRepository.GetFilteredAsync(parameters);
            
            return new PagedResult<Cycle>
            {
                Items = cycles,
                TotalItems = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }

        public async Task<Cycle> GetCycleByIdAsync(Guid id)
        {
            return await _cycleRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Cycle>> GetAllCyclesAsync()
        {
            return await _cycleRepository.GetAllAsync();
        }

        public async Task<Cycle> CreateCycleAsync(Cycle cycle)
        {
            return await _cycleRepository.AddAsync(cycle);
        }

        public async Task<Cycle> UpdateCycleAsync(Cycle cycle)
        {
            return await _cycleRepository.UpdateAsync(cycle);
        }

        public async Task<bool> DeleteCycleAsync(Guid id)
        {
            return await _cycleRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _cycleRepository.ExistsAsync(id);
        }
    }
}