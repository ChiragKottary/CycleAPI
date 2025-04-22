using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Service.Interface
{
    public interface ICustomerService
    {
        Task<CustomerDto?> GetCustomerByIdAsync(Guid id);
        Task<CustomerDto?> GetCustomerByEmailAsync(string email);
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm);
        Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto customerDto);
        Task<CustomerDto?> UpdateCustomerAsync(Guid id, CustomerUpdateDto customerDto);
        Task<bool> DeleteCustomerAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<CartDto?> GetCustomerActiveCartAsync(Guid customerId);
        Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(Guid customerId);
        Task<PagedResult<CustomerDto>> GetFilteredCustomersAsync(CustomerQueryParameters parameters);
        Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(Guid customerId);
        Task<bool> ValidateCustomerAsync(CustomerValidationDto validationDto);
    }
}
