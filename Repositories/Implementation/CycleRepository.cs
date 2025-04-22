using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CycleAPI.Repositories.Implementation
{
    public class CycleRepository : ICycleRepository
    {
        private readonly ApplicationDbContext _context;

        public CycleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cycle?> GetByIdAsync(Guid id)
        {
            return await _context.Cycles
                .Include(c => c.Brand)
                .Include(c => c.CycleType)
                .FirstOrDefaultAsync(c => c.CycleId == id);
        }

        public async Task<IEnumerable<Cycle>> GetAllAsync()
        {
            return await _context.Cycles
                .Include(c => c.Brand)
                .Include(c => c.CycleType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cycle>> GetByBrandIdAsync(Guid brandId)
        {
            return await _context.Cycles
                .Include(c => c.Brand)
                .Include(c => c.CycleType)
                .Where(c => c.BrandId == brandId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cycle>> GetByTypeIdAsync(Guid typeId)
        {
            return await _context.Cycles
                .Include(c => c.Brand)
                .Include(c => c.CycleType)
                .Where(c => c.TypeId == typeId)
                .ToListAsync();
        }

        public async Task<Cycle> AddAsync(Cycle cycle)
        {
            cycle.CreatedAt = DateTime.UtcNow;
            cycle.UpdatedAt = DateTime.UtcNow;
            await _context.Cycles.AddAsync(cycle);
            await _context.SaveChangesAsync();
            return cycle;
        }

        public async Task<Cycle> UpdateAsync(Cycle cycle)
        {
            cycle.UpdatedAt = DateTime.UtcNow;
            _context.Entry(cycle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return cycle;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var cycle = await GetByIdAsync(id);
            if (cycle == null)
                return false;

            _context.Cycles.Remove(cycle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Cycles.AnyAsync(c => c.CycleId == id);
        }

        public async Task<bool> UpdateStockAsync(Guid id, int quantity)
        {
            var cycle = await GetByIdAsync(id);
            if (cycle == null)
                return false;

            cycle.StockQuantity = quantity;
            cycle.UpdatedAt = DateTime.UtcNow;
            _context.Entry(cycle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<Cycle> Cycles, int TotalCount)> GetFilteredAsync(CycleQueryParameters parameters)
        {
            var query = _context.Cycles
                .Include(c => c.Brand)
                .Include(c => c.CycleType)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(c => c.ModelName.ToLower().Contains(searchTerm) ||
                                       c.Description.ToLower().Contains(searchTerm) ||
                                       c.Brand.BrandName.ToLower().Contains(searchTerm) ||
                                       c.CycleType.TypeName.ToLower().Contains(searchTerm));
            }

            if (parameters.BrandId.HasValue)
                query = query.Where(c => c.BrandId == parameters.BrandId);

            if (parameters.TypeId.HasValue)
                query = query.Where(c => c.TypeId == parameters.TypeId);

            if (parameters.MinPrice.HasValue)
                query = query.Where(c => c.Price >= parameters.MinPrice.Value);

            if (parameters.MaxPrice.HasValue)
                query = query.Where(c => c.Price <= parameters.MaxPrice.Value);

            if (parameters.MinStock.HasValue)
                query = query.Where(c => c.StockQuantity >= parameters.MinStock.Value);

            if (parameters.MaxStock.HasValue)
                query = query.Where(c => c.StockQuantity <= parameters.MaxStock.Value);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Apply pagination
            var skip = (parameters.Page - 1) * parameters.PageSize;
            query = query.Skip(skip).Take(parameters.PageSize);

            var cycles = await query.ToListAsync();
            return (cycles, totalCount);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        private static IQueryable<Cycle> ApplySorting(IQueryable<Cycle> query, BaseQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy))
                return query.OrderBy(c => c.ModelName);

            var isAscending = parameters.SortDirection == SortDirection.Ascending;
            Expression<Func<Cycle, object>> keySelector = parameters.SortBy.ToLower() switch
            {
                "name" => c => c.ModelName,
                "price" => c => c.Price,
                "stock" => c => c.StockQuantity,
                "brand" => c => c.Brand.BrandName,
                "type" => c => c.CycleType.TypeName,
                "createdat" => c => c.CreatedAt,
                "updatedat" => c => c.UpdatedAt,
                _ => c => c.ModelName
            };

            return isAscending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }
    }
}
