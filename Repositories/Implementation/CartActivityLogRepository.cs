using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CycleAPI.Repositories.Implementation
{
    public class CartActivityLogRepository : ICartActivityLogRepository
    {
        private readonly ApplicationDbContext _context;

        public CartActivityLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartActivityLog> AddAsync(CartActivityLog log)
        {
            log.CreatedAt = DateTime.UtcNow;
            await _context.CartActivityLogs.AddAsync(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<IEnumerable<CartActivityLog>> GetByCartIdAsync(Guid cartId)
        {
            return await _context.CartActivityLogs
                .Include(l => l.Cart)
                .Include(l => l.Customer)
                .Include(l => l.Cycle)
                .Include(l => l.User)
                .Where(l => l.CartId == cartId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CartActivityLog>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.CartActivityLogs
                .Include(l => l.Cart)
                .Include(l => l.Customer)
                .Include(l => l.Cycle)
                .Include(l => l.User)
                .Where(l => l.CustomerId == customerId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<CartActivityLog> Logs, int TotalCount)> GetFilteredAsync(CartActivityLogQueryParameters parameters)
        {
            var query = _context.CartActivityLogs
                .Include(l => l.Cart)
                .Include(l => l.Customer)
                .Include(l => l.Cycle)
                .Include(l => l.User)
                .AsQueryable();

            // Apply filters
            if (parameters.CartId.HasValue)
                query = query.Where(l => l.CartId == parameters.CartId.Value);

            if (parameters.CustomerId.HasValue)
                query = query.Where(l => l.CustomerId == parameters.CustomerId.Value);

            if (parameters.CycleId.HasValue)
                query = query.Where(l => l.CycleId == parameters.CycleId.Value);

            if (parameters.UserId.HasValue)
                query = query.Where(l => l.UserId == parameters.UserId.Value);

            if (!string.IsNullOrWhiteSpace(parameters.Action))
                query = query.Where(l => l.Action.ToLower().Contains(parameters.Action.ToLower()));

            if (parameters.FromDate.HasValue)
                query = query.Where(l => l.CreatedAt >= parameters.FromDate.Value);

            if (parameters.ToDate.HasValue)
                query = query.Where(l => l.CreatedAt <= parameters.ToDate.Value);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Apply pagination
            var skip = (parameters.Page - 1) * parameters.PageSize;
            query = query.Skip(skip).Take(parameters.PageSize);

            var logs = await query.ToListAsync();
            return (logs, totalCount);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        private static IQueryable<CartActivityLog> ApplySorting(IQueryable<CartActivityLog> query, BaseQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy))
                return query.OrderByDescending(l => l.CreatedAt);

            var isAscending = parameters.SortDirection == SortDirection.Ascending;
            Expression<Func<CartActivityLog, object>> keySelector = parameters.SortBy.ToLower() switch
            {
                "timestamp" => l => l.CreatedAt,
                "action" => l => l.Action,
                "customername" => l => l.Customer.LastName,
                "cyclename" => l => l.Cycle.ModelName,
                "quantity" => l => l.Quantity,
                _ => l => l.CreatedAt
            };

            return isAscending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }
    }
}