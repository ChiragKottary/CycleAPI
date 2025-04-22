using CycleAPI.Models.DTO;
using CycleAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CycleAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerServiceController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerServiceController> _logger;

        public CustomerServiceController(ICustomerService customerService, ILogger<CustomerServiceController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpGet("profile")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetCustomerProfile()
        {
            var customerId = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized();
            }

            _logger.LogInformation($"Getting profile for customer with ID: {customerId}");

            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(Guid.Parse(customerId));
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("profile")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerDto>> UpdateCustomerProfile([FromBody] CustomerUpdateDto customerDto)
        {
            var customerId = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized();
            }

            _logger.LogInformation($"Updating profile for customer with ID: {customerId}");

            try
            {
                var updatedCustomer = await _customerService.UpdateCustomerAsync(Guid.Parse(customerId), customerDto);
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

        [HttpGet("cart")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> GetActiveCart()
        {
            var customerId = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized();
            }

            _logger.LogInformation($"Getting active cart for customer with ID: {customerId}");

            try
            {
                var cart = await _customerService.GetCustomerActiveCartAsync(Guid.Parse(customerId));
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("statistics")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerStatisticsDto>> GetCustomerStatistics()
        {
            var customerId = User.FindFirst("CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                return Unauthorized();
            }

            _logger.LogInformation($"Getting statistics for customer with ID: {customerId}");

            try
            {
                var statistics = await _customerService.GetCustomerStatisticsAsync(Guid.Parse(customerId));
                return Ok(statistics);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
} 