using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CycleAPI.Repositories.Implementation
{
    public class BrandRepository : IBrandRepository
    {
        private readonly ApplicationDbContext _context;

        public BrandRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Brand?> GetByIdAsync(Guid id)
        {
            return await _context.Brands
                .Include(b => b.Cycles)
                .FirstOrDefaultAsync(b => b.BrandId == id);
        }

        public async Task<IEnumerable<Brand>> GetAllAsync()
        {
            return await _context.Brands
                .Include(b => b.Cycles)
                .ToListAsync();
        }

        public async Task<Brand> AddAsync(Brand brand)
        {
            brand.CreatedAt = DateTime.UtcNow;
            brand.UpdatedAt = DateTime.UtcNow;
            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task<Brand> UpdateAsync(Brand brand)
        {
            brand.UpdatedAt = DateTime.UtcNow;
            _context.Entry(brand).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var brand = await GetByIdAsync(id);
            if (brand == null)
                return false;

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Brands.AnyAsync(b => b.BrandId == id);
        }

        public async Task<(IEnumerable<Brand> Brands, int TotalCount)> GetFilteredBrandsAsync(BrandQueryParameters parameters)
        {
            var query = _context.Brands
                .Include(b => b.Cycles)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(b => b.BrandName.ToLower().Contains(searchTerm) ||
                                       b.Description.ToLower().Contains(searchTerm));
            }

            if (parameters.MinCycles.HasValue)
                query = query.Where(b => b.Cycles.Count >= parameters.MinCycles.Value);

            if (parameters.MaxCycles.HasValue)
                query = query.Where(b => b.Cycles.Count <= parameters.MaxCycles.Value);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Apply pagination
            var skip = (parameters.Page - 1) * parameters.PageSize;
            query = query.Skip(skip).Take(parameters.PageSize);

            var brands = await query.ToListAsync();
            return (brands, totalCount);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        private static IQueryable<Brand> ApplySorting(IQueryable<Brand> query, BaseQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy))
                return query.OrderBy(b => b.BrandName);

            var isAscending = parameters.SortDirection == SortDirection.Ascending;
            Expression<Func<Brand, object>> keySelector = parameters.SortBy.ToLower() switch
            {
                "name" => b => b.BrandName,
                "cyclecount" => b => b.Cycles.Count,
                "createdat" => b => b.CreatedAt,
                "updatedat" => b => b.UpdatedAt,
                _ => b => b.BrandName
            };

            return isAscending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }
    }
}
