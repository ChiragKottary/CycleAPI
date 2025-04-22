using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Models.Enums;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace CycleAPI.Repositories.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Transaction has not been started");

            try
            {
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Transaction has not been started");

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Cycle)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Cycle)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Cycle)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Cycle)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            await BeginTransactionAsync();
            try
            {
                order.CreatedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();
                await CommitAsync();
                return order;
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var order = await GetOrderByIdAsync(id);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Orders.AnyAsync(o => o.OrderId == id);
        }

        public async Task<bool> UpdateStatusAsync(Guid id, OrderStatus status)
        {
            var order = await GetOrderByIdAsync(id);
            if (order == null)
                return false;

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetFilteredAsync(OrderQueryParameters parameters)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Cycle)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(o => o.OrderNumber.ToLower().Contains(searchTerm) ||
                                       o.Customer.FirstName.ToLower().Contains(searchTerm) ||
                                       o.Customer.LastName.ToLower().Contains(searchTerm) ||
                                       o.Customer.Email.ToLower().Contains(searchTerm));
            }

            if (parameters.CustomerId.HasValue)
                query = query.Where(o => o.CustomerId == parameters.CustomerId.Value);

            if (!string.IsNullOrWhiteSpace(parameters.OrderNumber))
                query = query.Where(o => o.OrderNumber.Contains(parameters.OrderNumber));

            if (parameters.Status.HasValue)
                query = query.Where(o => o.Status == parameters.Status);

            if (parameters.MinAmount.HasValue)
                query = query.Where(o => o.TotalAmount >= parameters.MinAmount.Value);

            if (parameters.MaxAmount.HasValue)
                query = query.Where(o => o.TotalAmount <= parameters.MaxAmount.Value);

            if (parameters.FromDate.HasValue)
                query = query.Where(o => o.OrderDate >= parameters.FromDate.Value);

            if (parameters.ToDate.HasValue)
                query = query.Where(o => o.OrderDate <= parameters.ToDate.Value);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Apply pagination
            var skip = (parameters.Page - 1) * parameters.PageSize;
            query = query.Skip(skip).Take(parameters.PageSize);

            var orders = await query.ToListAsync();
            return (orders, totalCount);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        private static IQueryable<Order> ApplySorting(IQueryable<Order> query, BaseQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy))
                return query.OrderByDescending(o => o.OrderDate);

            var isAscending = parameters.SortDirection == SortDirection.Ascending;
            Expression<Func<Order, object>> keySelector = parameters.SortBy.ToLower() switch
            {
                "ordernumber" => o => o.OrderNumber,
                "customername" => o => o.Customer.LastName,
                "orderdate" => o => o.OrderDate,
                "status" => o => o.Status,
                "totalamount" => o => o.TotalAmount,
                "itemcount" => o => o.OrderItems.Count,
                "createdat" => o => o.CreatedAt,
                "updatedat" => o => o.UpdatedAt,
                _ => o => o.OrderDate
            };

            return isAscending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }
    }
}