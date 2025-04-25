using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CycleAPI.Repositories.Implementation
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByIdAsync(Guid cartId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Cycle)
                        .ThenInclude(c => c.Brand)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Cycle)
                        .ThenInclude(c => c.CycleType)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.CartId == cartId);
        }

        public async Task<Cart?> GetActiveByCustomerIdAsync(Guid customerId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Cycle)
                        .ThenInclude(c => c.Brand)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Cycle)
                        .ThenInclude(c => c.CycleType)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsActive);
        }

        public async Task<IEnumerable<Cart>> GetAllActiveAsync()
        {
            return await _context.Carts
                .Include(c => c.Customer)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Cycle)
                        .ThenInclude(c => c.Brand)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Cycle)
                        .ThenInclude(c => c.CycleType)
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cart>> SearchByCustomerNameAsync(string name)
        {
            return await _context.Carts
                .Include(c => c.Customer)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Cycle)
                        .ThenInclude(c => c.Brand)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Cycle)
                        .ThenInclude(c => c.CycleType)
                .Where(c => c.IsActive &&
                       (c.Customer.FirstName.Contains(name) ||
                        c.Customer.LastName.Contains(name)))
                .ToListAsync();
        }

        public async Task<Cart> AddAsync(Cart cart)
        {
            cart.CreatedAt = DateTime.UtcNow;
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<Cart> UpdateAsync(Cart cart)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            _context.Entry(cart).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<bool> DeleteAsync(Guid cartId)
        {
            var cart = await GetByIdAsync(cartId);
            if (cart == null)
                return false;

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid cartId)
        {
            return await _context.Carts.AnyAsync(c => c.CartId == cartId);
        }

        public async Task<(IEnumerable<Cart> Carts, int TotalCount)> GetFilteredAsync(CartQueryParameters parameters)
        {
            var query = _context.Carts
                .Include(c => c.Customer)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Cycle)
                        .ThenInclude(c => c.Brand)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Cycle)
                        .ThenInclude(c => c.CycleType)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (parameters.IsActive.HasValue)
                query = query.Where(c => c.IsActive == parameters.IsActive.Value);

            if (parameters.MinAmount.HasValue)
                query = query.Where(c => c.CartItems.Sum(ci => ci.Cycle.Price * ci.Quantity) >= parameters.MinAmount.Value);

            if (parameters.MaxAmount.HasValue)
                query = query.Where(c => c.CartItems.Sum(ci => ci.Cycle.Price * ci.Quantity) <= parameters.MaxAmount.Value);

            if (parameters.CreatedFrom.HasValue)
                query = query.Where(c => c.CreatedAt >= parameters.CreatedFrom.Value);

            if (parameters.CreatedTo.HasValue)
                query = query.Where(c => c.CreatedAt <= parameters.CreatedTo.Value);

            if (!string.IsNullOrWhiteSpace(parameters.CustomerName))
            {
                var searchTerm = parameters.CustomerName.ToLower();
                query = query.Where(c => c.Customer.FirstName.ToLower().Contains(searchTerm) ||
                                       c.Customer.LastName.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(c => c.Customer.FirstName.ToLower().Contains(searchTerm) ||
                                       c.Customer.LastName.ToLower().Contains(searchTerm) ||
                                       (c.Notes != null && c.Notes.ToLower().Contains(searchTerm)));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Apply pagination
            var skip = (parameters.Page - 1) * parameters.PageSize;
            query = query.Skip(skip).Take(parameters.PageSize);

            var carts = await query.ToListAsync();
            return (carts, totalCount);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        private static IQueryable<Cart> ApplySorting(IQueryable<Cart> query, BaseQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy))
                return query.OrderByDescending(c => c.UpdatedAt);

            var isAscending = parameters.SortDirection == SortDirection.Ascending;
            Expression<Func<Cart, object>> keySelector = parameters.SortBy.ToLower() switch
            {
                "customername" => c => c.Customer.LastName,
                "createdat" => c => c.CreatedAt,
                "updatedat" => c => c.UpdatedAt,
                "totalamount" => c => c.CartItems.Sum(ci => ci.Cycle.Price * ci.Quantity),
                "totalitems" => c => c.CartItems.Sum(ci => ci.Quantity),
                _ => c => c.UpdatedAt
            };

            return isAscending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }

        public async Task<Cart> CreateCartAsync(Guid customerId, string sessionId)
        {
            var cart = new Cart
            {
                CustomerId = customerId,
                SessionId = sessionId,
                Notes = string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
            return cart;
        }
    }
}
