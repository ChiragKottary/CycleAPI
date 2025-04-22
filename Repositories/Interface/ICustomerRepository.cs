using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;

namespace CycleAPI.Repositories.Interface
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetCustomerByIdAsync(Guid id);
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(Guid id);
        Task<bool> CustomerExistsAsync(Guid id);
        Task<bool> EmailExistsAsync(string email);
        Task<Cart?> GetActiveCartAsync(Guid customerId);
        Task<(IEnumerable<Customer> Customers, int TotalCount)> GetFilteredAsync(CustomerQueryParameters parameters);
        Task<bool> SaveChangesAsync();
    }
}
