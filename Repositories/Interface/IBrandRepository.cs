using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;

namespace CycleAPI.Repositories.Interface
{
    public interface IBrandRepository
    {
        Task<Brand?> GetByIdAsync(Guid id);
        Task<IEnumerable<Brand>> GetAllAsync();
        Task<Brand> AddAsync(Brand brand);
        Task<Brand> UpdateAsync(Brand brand);
        Task<bool> DeleteAsync(Guid id);
        Task<(IEnumerable<Brand> Brands, int TotalCount)> GetFilteredBrandsAsync(BrandQueryParameters parameters);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}
