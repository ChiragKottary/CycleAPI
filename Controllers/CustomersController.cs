using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using CycleAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CycleAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            ICustomerService customerService,
            ITokenRepository tokenRepository,
            ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _tokenRepository = tokenRepository;
            _logger = logger;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CustomerDto>>> GetFilteredCustomers([FromQuery] CustomerQueryParameters parameters)
        {
            _logger.LogInformation($"Getting filtered customers. Page: {parameters.Page}, PageSize: {parameters.PageSize}");
            var pagedResult = await _customerService.GetFilteredCustomersAsync(parameters);
            return Ok(pagedResult);
        }

        [HttpGet("{id:guid}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(Guid id)
        {
            _logger.LogInformation($"Getting customer with ID: {id}");

            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("email/{email}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetCustomerByEmail(string email)
        {
            _logger.LogInformation($"Getting customer with email: {email}");

            try
            {
                var customer = await _customerService.GetCustomerByEmailAsync(email);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("search")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> SearchCustomers([FromQuery] string searchTerm)
        {
            _logger.LogInformation($"Searching customers with term: {searchTerm}");

            var customers = await _customerService.SearchCustomersAsync(searchTerm);
            return Ok(customers);
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CustomerCreateDto customerDto)
        {
            _logger.LogInformation($"Creating new customer with email: {customerDto.Email}");

            try
            {
                var createdCustomer = await _customerService.CreateCustomerAsync(customerDto);
                return CreatedAtAction(nameof(GetCustomerById), new { id = createdCustomer.CustomerId }, createdCustomer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerDto>> UpdateCustomer(Guid id, [FromBody] CustomerUpdateDto customerDto)
        {
            _logger.LogInformation($"Updating customer with ID: {id}");

            try
            {
                var updatedCustomer = await _customerService.UpdateCustomerAsync(id, customerDto);
                return Ok(updatedCustomer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            _logger.LogInformation($"Deleting customer with ID: {id}");

            try
            {
                await _customerService.DeleteCustomerAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:guid}/cart")]
        //[Authorize(Roles = "Admin,Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> GetCustomerActiveCart(Guid id)
        {
            _logger.LogInformation($"Getting active cart for customer with ID: {id}");

            try
            {
                var cart = await _customerService.GetCustomerActiveCartAsync(id);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{id:guid}/statistics")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerStatisticsDto>> GetCustomerStatistics(Guid id)
        {
            _logger.LogInformation($"Getting statistics for customer with ID: {id}");

            try
            {
                var statistics = await _customerService.GetCustomerStatisticsAsync(id);
                return Ok(statistics);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> ValidateCustomer([FromBody] CustomerValidationDto validationDto)
        {
            _logger.LogInformation($"Validating customer with email: {validationDto.Email}");

            var isValid = await _customerService.ValidateCustomerAsync(validationDto);
            return Ok(isValid);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerAuthResponseDto>> Login([FromBody] CustomerAuthRequestDto request)
        {
            try
            {
                _logger.LogInformation($"Customer login attempt for email: {request.Email}");

                if (!ModelState.IsValid)
                {
                    return ValidationProblem(ModelState);
                }

                var (isValid, customer) = await _customerService.ValidateCustomerLoginAsync(request.Email, request.Password);
                
                if (!isValid || customer == null)
                {
                    _logger.LogWarning($"Invalid login attempt for email: {request.Email}");
                    ModelState.AddModelError("", "Email or password is incorrect.");
                    return ValidationProblem(ModelState);
                }

                try
                {
                    // Update last login date
                    await _customerService.UpdateCustomerLastLoginAsync(customer.CustomerId);

                    // Generate JWT token
                    var token = await _tokenRepository.CreateCustomerTokenAsync(customer);

                    var response = new CustomerAuthResponseDto
                    {
                        CustomerId = customer.CustomerId,
                        Email = customer.Email,
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Token = token
                    };

                    _logger.LogInformation($"Customer login successful for email: {request.Email}");
                    return Ok(response);
                }
                catch (Exception tokenEx)
                {
                    _logger.LogError($"Token generation failed for customer {request.Email}: {tokenEx.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during customer login for {request.Email}: {ex.Message}\nStack trace: {ex.StackTrace}");
                return StatusCode(500, "An error occurred during login.");
            }
        }
    }
}
