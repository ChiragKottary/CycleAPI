using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using CycleAPI.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace CycleAPI.Service.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            ICustomerRepository customerRepository,
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllCustomersAsync();
            return customers.Select(MapToCustomerDto);
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(Guid id)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {id} not found");

            return MapToCustomerDto(customer);
        }

        public async Task<CustomerDto> GetCustomerByEmailAsync(string email)
        {
            var customer = await _customerRepository.GetCustomerByEmailAsync(email);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with email {email} not found");

            return MapToCustomerDto(customer);
        }

        public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm)
        {
            var customers = await _customerRepository.SearchCustomersAsync(searchTerm);
            return customers.Select(MapToCustomerDto);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto customerDto)
        {
            // Validate email uniqueness
            if (await _customerRepository.EmailExistsAsync(customerDto.Email))
                throw new InvalidOperationException($"Email {customerDto.Email} is already registered");

            var customer = new Customer
            {
                CustomerId = Guid.NewGuid(),
                FirstName = customerDto.FirstName,
                LastName = customerDto.LastName,
                Email = customerDto.Email,
                Phone = customerDto.Phone,
                Address = customerDto.Address,
                City = customerDto.City,
                State = customerDto.State,
                PostalCode = customerDto.PostalCode
            };

            customer = await _customerRepository.CreateCustomerAsync(customer);
            await _customerRepository.SaveChangesAsync();

            return MapToCustomerDto(customer);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(Guid id, CustomerUpdateDto customerDto)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {id} not found");

            // Check email uniqueness if it's being changed
            if (!string.IsNullOrEmpty(customerDto.Email) && 
                customerDto.Email.ToLower() != customer.Email.ToLower() &&
                await _customerRepository.EmailExistsAsync(customerDto.Email))
            {
                throw new InvalidOperationException($"Email {customerDto.Email} is already registered");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(customerDto.FirstName))
                customer.FirstName = customerDto.FirstName;
            if (!string.IsNullOrEmpty(customerDto.LastName))
                customer.LastName = customerDto.LastName;
            if (!string.IsNullOrEmpty(customerDto.Email))
                customer.Email = customerDto.Email;
            if (!string.IsNullOrEmpty(customerDto.Phone))
                customer.Phone = customerDto.Phone;
            if (!string.IsNullOrEmpty(customerDto.Address))
                customer.Address = customerDto.Address;
            if (!string.IsNullOrEmpty(customerDto.City))
                customer.City = customerDto.City;
            if (!string.IsNullOrEmpty(customerDto.State))
                customer.State = customerDto.State;
            if (!string.IsNullOrEmpty(customerDto.PostalCode))
                customer.PostalCode = customerDto.PostalCode;

            await _customerRepository.UpdateCustomerAsync(customer);
            await _customerRepository.SaveChangesAsync();

            return MapToCustomerDto(customer);
        }

        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            if (!await _customerRepository.CustomerExistsAsync(id))
                throw new KeyNotFoundException($"Customer with ID {id} not found");

            var success = await _customerRepository.DeleteCustomerAsync(id);
            if (success)
                await _customerRepository.SaveChangesAsync();

            return success;
        }

        public async Task<CartDto> GetCustomerActiveCartAsync(Guid customerId)
        {
            if (!await _customerRepository.CustomerExistsAsync(customerId))
                throw new KeyNotFoundException($"Customer with ID {customerId} not found");

            var cart = await _cartRepository.GetActiveByCustomerIdAsync(customerId);
            if (cart == null)
                throw new KeyNotFoundException($"No active cart found for customer {customerId}");

            return MapToCartDto(cart);
        }

        public async Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(Guid customerId)
        {
            if (!await _customerRepository.CustomerExistsAsync(customerId))
                throw new KeyNotFoundException($"Customer with ID {customerId} not found");

            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            var orders = await _orderRepository.GetByCustomerIdAsync(customerId);

            var statistics = new CustomerStatisticsDto
            {
                CustomerId = (int)customerId.GetHashCode(),
                CustomerName = $"{customer.FirstName} {customer.LastName}",
                TotalOrders = orders.Count(),
                TotalSpent = orders.Sum(o => o.TotalAmount),
                FirstOrderDate = orders.MinBy(o => o.OrderDate)?.OrderDate,
                LastOrderDate = orders.MaxBy(o => o.OrderDate)?.OrderDate,
            };

            // Calculate most purchased brand and cycle type
            var orderItems = orders.SelectMany(o => o.OrderItems);
            
            var brandGroups = orderItems
                .GroupBy(oi => oi.Cycle.Brand.BrandName)
                .Select(g => new { Brand = g.Key, TotalQuantity = g.Sum(oi => oi.Quantity) });

            var typeGroups = orderItems
                .GroupBy(oi => oi.Cycle.CycleType.TypeName)
                .Select(g => new { Type = g.Key, TotalQuantity = g.Sum(oi => oi.Quantity) });

            statistics.MostPurchasedBrand = brandGroups
                .OrderByDescending(g => g.TotalQuantity)
                .FirstOrDefault()?.Brand ?? "N/A";

            statistics.MostPurchasedCycleType = typeGroups
                .OrderByDescending(g => g.TotalQuantity)
                .FirstOrDefault()?.Type ?? "N/A";

            return statistics;
        }

        public async Task<bool> ValidateCustomerAsync(CustomerValidationDto validationDto)
        {
            var customer = await _customerRepository.GetCustomerByEmailAsync(validationDto.Email);
            if (customer == null)
                return false;

            return customer.Phone == validationDto.Phone &&
                   customer.PostalCode == validationDto.PostalCode;
        }

        public async Task<PagedResult<CustomerDto>> GetFilteredCustomersAsync(CustomerQueryParameters parameters)
        {
            var (customers, totalCount) = await _customerRepository.GetFilteredAsync(parameters);
            
            return new PagedResult<CustomerDto>
            {
                Items = customers.Select(MapToCustomerDto),
                TotalItems = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }

        private static CustomerDto MapToCustomerDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerId = customer.CustomerId.GetHashCode(),
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                PostalCode = customer.PostalCode,
                RegistrationDate = customer.RegistrationDate,
                UpdatedAt = customer.UpdatedAt,
                HasActiveCart = customer.Carts?.Any(c => c.IsActive) ?? false,
                TotalOrders = 0 // This would need to be populated if we want to track total orders
            };
        }

        private static CartDto MapToCartDto(Cart cart)
        {
            return new CartDto
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                CustomerName = $"{cart.Customer?.FirstName} {cart.Customer?.LastName}",
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                IsActive = cart.IsActive,
                SessionId = cart.SessionId,
                Notes = cart.Notes,
                TotalAmount = cart.CartItems?.Sum(ci => ci.Cycle.Price * ci.Quantity) ?? 0,
                TotalItems = cart.CartItems?.Sum(ci => ci.Quantity) ?? 0,
                CartItems = cart.CartItems?.Select(ci => new CartItemDto
                {
                    CartItemId = ci.CartItemId,
                    CartId = ci.CartId,
                    CycleId = ci.CycleId,
                    CycleName = ci.Cycle.ModelName,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Cycle.Price,
                    Subtotal = ci.Cycle.Price * ci.Quantity
                }).ToList() ?? new List<CartItemDto>()
            };
        }

        Task<IEnumerable<OrderDto>> ICustomerService.GetCustomerOrdersAsync(Guid customerId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _customerRepository.CustomerExistsAsync(id);
        }
    }
}