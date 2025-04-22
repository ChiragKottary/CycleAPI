using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CycleAPI.Repositories.Implementation
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetCustomerByIdAsync(Guid id)
        {
            return await _context.Customers
                .Include(c => c.Carts)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers
                .Include(c => c.Carts)
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Include(c => c.Carts)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _context.Customers
                .Include(c => c.Carts)
                .Where(c => c.FirstName.ToLower().Contains(term) ||
                           c.LastName.ToLower().Contains(term) ||
                           c.Email.ToLower().Contains(term) ||
                           c.Phone.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            customer.CreatedAt = DateTime.UtcNow;
            customer.UpdatedAt = DateTime.UtcNow;
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            customer.UpdatedAt = DateTime.UtcNow;
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            var customer = await GetCustomerByIdAsync(id);
            if (customer == null)
                return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CustomerExistsAsync(Guid id)
        {
            return await _context.Customers.AnyAsync(c => c.CustomerId == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Customers.AnyAsync(c => c.Email.ToLower() == email.ToLower());
        }

        public async Task<Cart?> GetActiveCartAsync(Guid customerId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Cycle)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsActive);
        }

        public async Task<(IEnumerable<Customer> Customers, int TotalCount)> GetFilteredAsync(CustomerQueryParameters parameters)
        {
            var query = _context.Customers
                .Include(c => c.Carts)
                .Include(c => c.Orders)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(c => c.FirstName.ToLower().Contains(searchTerm) ||
                                       c.LastName.ToLower().Contains(searchTerm) ||
                                       c.Email.ToLower().Contains(searchTerm) ||
                                       c.Phone.Contains(parameters.SearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Email))
            {
                query = query.Where(c => c.Email.ToLower().Contains(parameters.Email.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Phone))
            {
                query = query.Where(c => c.Phone.Contains(parameters.Phone));
            }

            if (parameters.HasActiveCart.HasValue)
            {
                query = query.Where(c => c.Carts.Any(cart => cart.IsActive) == parameters.HasActiveCart.Value);
            }

            if (parameters.MinOrders.HasValue)
            {
                query = query.Where(c => c.Orders.Count >= parameters.MinOrders.Value);
            }

            if (parameters.MaxOrders.HasValue)
            {
                query = query.Where(c => c.Orders.Count <= parameters.MaxOrders.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Apply pagination
            var skip = (parameters.Page - 1) * parameters.PageSize;
            query = query.Skip(skip).Take(parameters.PageSize);

            var customers = await query.ToListAsync();
            return (customers, totalCount);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }

        private static IQueryable<Customer> ApplySorting(IQueryable<Customer> query, BaseQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy))
                return query.OrderBy(c => c.LastName);

            var isAscending = parameters.SortDirection == SortDirection.Ascending;
            Expression<Func<Customer, object>> keySelector = parameters.SortBy.ToLower() switch
            {
                "firstname" => c => c.FirstName,
                "lastname" => c => c.LastName,
                "email" => c => c.Email,
                "phone" => c => c.Phone,
                "ordercount" => c => c.Orders.Count,
                "createdat" => c => c.CreatedAt,
                "updatedat" => c => c.UpdatedAt,
                _ => c => c.LastName
            };

            return isAscending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }
    }
}
