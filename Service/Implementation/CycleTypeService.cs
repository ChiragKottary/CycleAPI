using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using CycleAPI.Service.Interface;

namespace CycleAPI.Service.Implementation
{
    public class CycleTypeService : ICycleTypeService
    {
        private readonly ICycleTypeRepository _cycleTypeRepository;
        private readonly ILogger<CycleTypeService> _logger;

        public CycleTypeService(ICycleTypeRepository cycleTypeRepository, ILogger<CycleTypeService> logger)
        {
            _cycleTypeRepository = cycleTypeRepository;
            _logger = logger;
        }

        public async Task<CycleType> CreateAsync(CycleType cycleType)
        {
            return await _cycleTypeRepository.AddAsync(cycleType);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _cycleTypeRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<CycleType>> GetAllAsync()
        {
            return await _cycleTypeRepository.GetAllAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            var cycleType = await _cycleTypeRepository.GetByIdAsync(id);
            return cycleType != null;
        }

        public async Task<CycleType> GetByIdAsync(Guid id)
        {
            return await _cycleTypeRepository.GetByIdAsync(id);
        }

        public async Task<CycleType> UpdateAsync(CycleType cycleType)
        {
            return await _cycleTypeRepository.UpdateAsync(cycleType);
        }

        public async Task<PagedResult<CycleType>> GetFilteredTypesAsync(CycleTypeQueryParameters parameters)
        {
            var (types, totalCount) = await _cycleTypeRepository.GetFilteredAsync(parameters);
            
            return new PagedResult<CycleType>
            {
                Items = types,
                TotalItems = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }
    }
}