using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CycleAPI.Repositories.Implementation
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderItem?> GetByIdAsync(Guid id)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Cycle)
                .FirstOrDefaultAsync(oi => oi.OrderItemId == id);
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Cycle)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetByCycleIdAsync(Guid cycleId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.CycleId == cycleId)
                .ToListAsync();
        }

        public async Task<OrderItem> AddAsync(OrderItem orderItem)
        {
            orderItem.CreatedAt = DateTime.UtcNow;
            orderItem.UpdatedAt = DateTime.UtcNow;
            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();
            return orderItem;
        }

        public async Task<OrderItem> UpdateAsync(OrderItem orderItem)
        {
            orderItem.UpdatedAt = DateTime.UtcNow;
            _context.Entry(orderItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return orderItem;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var orderItem = await GetByIdAsync(id);
            if (orderItem == null)
                return false;

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.OrderItems.AnyAsync(oi => oi.OrderItemId == id);
        }

        public async Task<(IEnumerable<OrderItem> Items, int TotalCount)> GetFilteredAsync(OrderItemQueryParameters parameters)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Cycle)
                .AsQueryable();

            // Apply filters
            if (parameters.OrderId.HasValue)
                query = query.Where(oi => oi.OrderId == parameters.OrderId.Value);

            if (parameters.CycleId.HasValue)
                query = query.Where(oi => oi.CycleId == parameters.CycleId.Value);

            if (parameters.MinPrice.HasValue)
                query = query.Where(oi => oi.UnitPrice >= parameters.MinPrice.Value);

            if (parameters.MaxPrice.HasValue)
                query = query.Where(oi => oi.UnitPrice <= parameters.MaxPrice.Value);

            if (parameters.MinQuantity.HasValue)
                query = query.Where(oi => oi.Quantity >= parameters.MinQuantity.Value);

            if (parameters.MaxQuantity.HasValue)
                query = query.Where(oi => oi.Quantity <= parameters.MaxQuantity.Value);

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(oi => oi.Cycle.ModelName.ToLower().Contains(searchTerm) ||
                                        oi.Order.OrderNumber.ToLower().Contains(searchTerm));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Apply pagination
            var skip = (parameters.Page - 1) * parameters.PageSize;
            query = query.Skip(skip).Take(parameters.PageSize);

            var items = await query.ToListAsync();
            return (items, totalCount);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        private static IQueryable<OrderItem> ApplySorting(IQueryable<OrderItem> query, BaseQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy))
                return query.OrderByDescending(oi => oi.CreatedAt);

            var isAscending = parameters.SortDirection == SortDirection.Ascending;
            Expression<Func<OrderItem, object>> keySelector = parameters.SortBy.ToLower() switch
            {
                "cyclename" => oi => oi.Cycle.ModelName,
                "ordernumber" => oi => oi.Order.OrderNumber,
                "unitprice" => oi => oi.UnitPrice,
                "quantity" => oi => oi.Quantity,
                "totalamount" => oi => oi.UnitPrice * oi.Quantity,
                "createdat" => oi => oi.CreatedAt,
                "updatedat" => oi => oi.UpdatedAt,
                _ => oi => oi.CreatedAt
            };

            return isAscending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }
    }
}