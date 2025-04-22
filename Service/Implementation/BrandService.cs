using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using CycleAPI.Service.Interface;

namespace CycleAPI.Service.Implementation
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly ILogger<BrandService> _logger;

        public BrandService(IBrandRepository brandRepository, ILogger<BrandService> logger)
        {
            _brandRepository = brandRepository;
            _logger = logger;
        }

        public async Task<Brand> CreateAsync(Brand brand)
        {
            return await _brandRepository.AddAsync(brand);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _brandRepository.DeleteAsync(id);
            return result != null;
        }

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            return await _brandRepository.GetAllAsync();
        }

        public async Task<Brand> GetByIdAsync(Guid id)
        {
            return await _brandRepository.GetByIdAsync(id);
        }
        public async Task<PagedResult<Brand>> GetFilteredBrandsAsync(BrandQueryParameters parameters)
        {
            var (brands, totalCount) = await _brandRepository.GetFilteredBrandsAsync(parameters);
            
            return new PagedResult<Brand>
            {
                Items = brands,
                TotalItems = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
            var brand = await _brandRepository.GetByIdAsync(id);
            return brand != null;
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Error checking if brand exists with ID: {Id}", id);
            return false;
            }
        }

        public async Task<Brand?> UpdateAsync(Brand brand)
        {
            try
            {
            return await _brandRepository.UpdateAsync(brand);
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Error updating brand with ID: {Id}", brand.BrandId);
            return null;
            }
        }
    }
}