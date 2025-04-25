using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using CycleAPI.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
                return null;

            return MapToCustomerDto(customer);
        }

        public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
        {
            var customer = await _customerRepository.GetCustomerByEmailAsync(email);
            if (customer == null)
                return null;

            return MapToCustomerDto(customer);
        }

        public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm)
        {
            var customers = await _customerRepository.SearchCustomersAsync(searchTerm);
            var customerDtos = new List<CustomerDto>();

            foreach (var customer in customers)
            {
                var customerDto = MapToCustomerDto(customer);
                var activeCart = await _cartRepository.GetActiveByCustomerIdAsync(customer.CustomerId);
                if (activeCart != null)
                {
                    customerDto.ActiveCart = MapToCartDto(activeCart);
                }
                customerDtos.Add(customerDto);
            }

            return customerDtos;
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
                PostalCode = customerDto.PostalCode,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(customerDto.Password),
                RegistrationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            customer = await _customerRepository.CreateCustomerAsync(customer);
            await _customerRepository.SaveChangesAsync();

            return MapToCustomerDto(customer);
        }

        public async Task<CustomerDto?> UpdateCustomerAsync(Guid id, CustomerUpdateDto customerDto)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
                return null;

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

        public async Task<CartDto?> GetCustomerActiveCartAsync(Guid customerId)
        {
            if (!await _customerRepository.CustomerExistsAsync(customerId))
                return null;

            var cart = await _cartRepository.GetActiveByCustomerIdAsync(customerId);
            if (cart == null)
            {
                // Create a new cart if none exists
                cart = await _cartRepository.CreateCartAsync(customerId, string.Empty);
            }

            return MapToCartDto(cart);
        }

        public async Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(Guid customerId)
        {
            if (!await _customerRepository.CustomerExistsAsync(customerId))
                throw new KeyNotFoundException($"Customer with ID {customerId} not found");

            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new InvalidOperationException($"Customer with ID {customerId} not found after existence check");
                
            var orders = await _orderRepository.GetByCustomerIdAsync(customerId);

            var statistics = new CustomerStatisticsDto
            {
                CustomerId = (int)customerId.GetHashCode(),
                CustomerName = $"{customer.FirstName ?? string.Empty} {customer.LastName ?? string.Empty}".Trim(),
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

        public async Task<bool> UpdateCustomerLastLoginAsync(Guid customerId)
        {
            try
            {
                _logger.LogInformation($"Updating last login time for customer ID: {customerId}");
                
                var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
                if (customer == null)
                {
                    _logger.LogWarning($"Cannot update last login: Customer not found with ID {customerId}");
                    return false;
                }

                customer.LastLoginDate = DateTime.UtcNow;
                customer.UpdatedAt = DateTime.UtcNow;
                
                await _customerRepository.UpdateCustomerAsync(customer);
                await _customerRepository.SaveChangesAsync();
                
                _logger.LogInformation($"Successfully updated last login time for customer ID: {customerId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating customer last login: {ex.Message}");
                throw;
            }
        }

        private static CustomerDto MapToCustomerDto(Customer customer)
        {
            if (customer == null) throw new ArgumentNullException(nameof(customer));
            
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName ?? string.Empty,
                LastName = customer.LastName ?? string.Empty,
                Email = customer.Email ?? string.Empty,
                Phone = customer.Phone ?? string.Empty,
                Address = customer.Address ?? string.Empty,
                City = customer.City ?? string.Empty,
                State = customer.State ?? string.Empty,
                PostalCode = customer.PostalCode ?? string.Empty,
                RegistrationDate = customer.RegistrationDate,
                UpdatedAt = customer.UpdatedAt,
                HasActiveCart = customer.Carts?.Any(c => c.IsActive) ?? false,
                TotalOrders = customer.Orders?.Count ?? 0,
                ActiveCart = customer.Carts?
                    .Where(c => c.IsActive)
                    .Select(cart => new CartDto
                    {
                        CartId = cart.CartId,
                        CustomerId = cart.CustomerId,
                        CustomerName = $"{customer.FirstName ?? string.Empty} {customer.LastName ?? string.Empty}".Trim(),
                        CreatedAt = cart.CreatedAt,
                        UpdatedAt = cart.UpdatedAt,
                        IsActive = cart.IsActive,
                        SessionId = cart.SessionId,
                        Notes = cart.Notes,
                        TotalAmount = cart.CartItems?.Sum(ci => ci.Cycle?.Price * ci.Quantity) ?? 0,
                        TotalItems = cart.CartItems?.Sum(ci => ci.Quantity) ?? 0,
                        CartItems = cart.CartItems?
                            .Select(ci => new CartItemDto
                            {
                                CartItemId = ci.CartItemId,
                                CartId = ci.CartId,
                                CycleId = ci.CycleId,
                                CycleName = ci.Cycle?.ModelName,
                                Quantity = ci.Quantity,
                                UnitPrice = ci.Cycle?.Price ?? 0,
                                TotalPrice = (ci.Cycle?.Price ?? 0) * ci.Quantity,
                                AddedAt = ci.AddedAt,
                                UpdatedAt = ci.UpdatedAt
                            }).ToList() ?? new List<CartItemDto>()
                    }).FirstOrDefault()
            };
        }

        private static CartDto MapToCartDto(Cart cart)
        {
            var customerName = cart.Customer != null ? $"{cart.Customer.FirstName} {cart.Customer.LastName}" : string.Empty;
            
            return new CartDto
            {
            CartId = cart.CartId,
            CustomerId = cart.CustomerId,
            CustomerName = customerName,
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
                CycleBrand = ci.Cycle.Brand?.BrandName,
                CycleType = ci.Cycle.CycleType?.TypeName,
                CycleDescription = ci.Cycle.Description,
                CycleImage = ci.Cycle.ImageUrl,
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

        public async Task<(bool isValid, CustomerDto? customer)> ValidateCustomerLoginAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation($"Validating customer login for email: {email}");

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Login attempt with empty email or password");
                    return (false, null);
                }

                var customer = await _customerRepository.GetCustomerByEmailAsync(email);
                
                if (customer == null)
                {
                    _logger.LogWarning($"Login attempt failed: Customer not found with email {email}");
                    return (false, null);
                }

                if (!customer.IsActive)
                {
                    _logger.LogWarning($"Login attempt failed: Customer account is inactive for email {email}");
                    return (false, null);
                }

                if (string.IsNullOrEmpty(customer.PasswordHash))
                {
                    _logger.LogError($"Customer {email} has no password hash");
                    return (false, null);
                }

                bool passwordValid = BCrypt.Net.BCrypt.Verify(password, customer.PasswordHash);
                if (!passwordValid)
                {
                    _logger.LogWarning($"Login attempt failed: Invalid password for customer {email}");
                    return (false, null);
                }

                var customerDto = MapToCustomerDto(customer);
                _logger.LogInformation($"Customer login validation successful for email: {email}");
                return (true, customerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during customer login validation: {ex.Message}");
                throw;
            }
        }
    }
}