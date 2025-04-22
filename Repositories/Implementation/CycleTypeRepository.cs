using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CycleAPI.Repositories.Implementation
{
    public class CycleTypeRepository : ICycleTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public CycleTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CycleType?> GetByIdAsync(Guid id)
        {
            return await _context.CycleTypes
                .Include(ct => ct.TypeName)
                .FirstOrDefaultAsync(ct => ct.TypeId == id);
        }

        public async Task<IEnumerable<CycleType>> GetAllAsync()
        {
            return await _context.CycleTypes
                .Include(ct => ct.TypeName)
                .ToListAsync();
        }

        public async Task<CycleType> AddAsync(CycleType cycleType)
        {
            cycleType.CreatedAt = DateTime.UtcNow;
            cycleType.UpdatedAt = DateTime.UtcNow;
            await _context.CycleTypes.AddAsync(cycleType);
            await _context.SaveChangesAsync();
            return cycleType;
        }

        public async Task<CycleType> UpdateAsync(CycleType cycleType)
        {
            cycleType.UpdatedAt = DateTime.UtcNow;
            _context.Entry(cycleType).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return cycleType;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var cycleType = await GetByIdAsync(id);
            if (cycleType == null)
                return false;

            _context.CycleTypes.Remove(cycleType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.CycleTypes.AnyAsync(ct => ct.TypeId == id);
        }

        public async Task<(IEnumerable<CycleType> Types, int TotalCount)> GetFilteredAsync(CycleTypeQueryParameters parameters)
        {
            var query = _context.CycleTypes
                .Include(ct => ct.TypeName)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(ct => ct.TypeName.ToLower().Contains(searchTerm) ||
                                        ct.Description.ToLower().Contains(searchTerm));
            }



            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Apply pagination
            var skip = (parameters.Page - 1) * parameters.PageSize;
            query = query.Skip(skip).Take(parameters.PageSize);

            var types = await query.ToListAsync();
            return (types, totalCount);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        private static IQueryable<CycleType> ApplySorting(IQueryable<CycleType> query, BaseQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy))
                return query.OrderBy(ct => ct.TypeName);

            var isAscending = parameters.SortDirection == SortDirection.Ascending;
            Expression<Func<CycleType, object>> keySelector = parameters.SortBy.ToLower() switch
            {
                "name" => ct => ct.TypeName,
                "createdat" => ct => ct.CreatedAt,
                "updatedat" => ct => ct.UpdatedAt,
                _ => ct => ct.TypeName
            };

            return isAscending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }
    }
}
