using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CycleAPI.Repositories.Implementation
{
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly ApplicationDbContext _context;

        public StockMovementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StockMovement?> GetByIdAsync(Guid id)
        {
            return await _context.StockMovement
                .Include(sm => sm.Cycle)
                .Include(sm => sm.User)
                .FirstOrDefaultAsync(sm => sm.MovementId == id);
        }

        public async Task<IEnumerable<StockMovement>> GetAllAsync()
        {
            return await _context.StockMovement
                .Include(sm => sm.Cycle)
                .Include(sm => sm.User)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetByCycleIdAsync(Guid cycleId)
        {
            return await _context.StockMovement
                .Include(sm => sm.User)
                .Where(sm => sm.CycleId == cycleId)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetByUserIdAsync(Guid userId)
        {
            return await _context.StockMovement
                .Include(sm => sm.Cycle)
                .Where(sm => sm.UserId == userId)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<StockMovement> AddAsync(StockMovement stockMovement)
        {
            stockMovement.MovementDate = DateTime.UtcNow;
            stockMovement.UpdatedAt = DateTime.UtcNow;
            await _context.StockMovement.AddAsync(stockMovement);
            await _context.SaveChangesAsync();
            return stockMovement;
        }

        public async Task<StockMovement> UpdateAsync(StockMovement stockMovement)
        {
            stockMovement.UpdatedAt = DateTime.UtcNow;
            _context.Entry(stockMovement).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return stockMovement;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var stockMovement = await GetByIdAsync(id);
            if (stockMovement == null)
                return false;

            _context.StockMovement.Remove(stockMovement);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.StockMovement.AnyAsync(sm => sm.MovementId == id);
        }

        public async Task<int> GetCurrentStockLevelAsync(Guid cycleId)
        {
            var movements = await _context.StockMovement
                .Where(sm => sm.CycleId == cycleId)
                .ToListAsync();

            return movements.Sum(sm => sm.Quantity);
        }

        public async Task<(IEnumerable<StockMovement> Movements, int TotalCount)> GetFilteredAsync(StockMovementQueryParameters parameters)
        {
            var query = _context.StockMovement
                .Include(sm => sm.Cycle)
                .Include(sm => sm.User)
                .AsQueryable();

            // Apply filters
            if (parameters.CycleId.HasValue)
                query = query.Where(sm => sm.CycleId == parameters.CycleId);

            if (parameters.UserId.HasValue)
                query = query.Where(sm => sm.UserId == parameters.UserId);

            if (parameters.MovementType.HasValue)
                query = query.Where(sm => sm.MovementType == parameters.MovementType);

            if (parameters.MinQuantity.HasValue)
                query = query.Where(sm => sm.Quantity >= parameters.MinQuantity.Value);

            if (parameters.MaxQuantity.HasValue)
                query = query.Where(sm => sm.Quantity <= parameters.MaxQuantity.Value);

            if (parameters.FromDate.HasValue)
                query = query.Where(sm => sm.MovementDate >= parameters.FromDate.Value);

            if (parameters.ToDate.HasValue)
                query = query.Where(sm => sm.MovementDate <= parameters.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(sm => sm.Cycle.ModelName.ToLower().Contains(searchTerm) ||
                                        sm.User.FirstName.ToLower().Contains(searchTerm) ||
                                        sm.User.LastName.ToLower().Contains(searchTerm) ||
                                        sm.Notes.ToLower().Contains(searchTerm));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Apply pagination
            var skip = (parameters.Page - 1) * parameters.PageSize;
            query = query.Skip(skip).Take(parameters.PageSize);

            var movements = await query.ToListAsync();
            return (movements, totalCount);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        private static IQueryable<StockMovement> ApplySorting(IQueryable<StockMovement> query, BaseQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy))
                return query.OrderByDescending(sm => sm.MovementDate);

            var isAscending = parameters.SortDirection == SortDirection.Ascending;
            Expression<Func<StockMovement, object>> keySelector = parameters.SortBy.ToLower() switch
            {
                "cyclename" => sm => sm.Cycle.ModelName,
                "username" => sm => sm.User.LastName,
                "quantity" => sm => sm.Quantity,
                "movementtype" => sm => sm.MovementType,
                "movementdate" => sm => sm.MovementDate,
                "updatedat" => sm => sm.UpdatedAt,
                _ => sm => sm.MovementDate
            };

            return isAscending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }
    }
}