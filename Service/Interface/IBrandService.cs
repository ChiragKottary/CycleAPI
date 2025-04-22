using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Service.Interface
{
    public interface IBrandService
    {
        Task<Brand?> GetByIdAsync(Guid id);
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        Task<Brand> CreateAsync(Brand brand);
        Task<Brand?> UpdateAsync(Brand brand);
        Task<bool> DeleteAsync(Guid id);
        Task<PagedResult<Brand>> GetFilteredBrandsAsync(BrandQueryParameters parameters);
        Task<bool> ExistsAsync(Guid id);
    }
}