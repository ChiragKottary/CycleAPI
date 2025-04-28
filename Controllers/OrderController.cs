using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Models.Enums;
using CycleAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CycleAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<OrderDto>>> GetFilteredOrders([FromQuery] OrderQueryParameters parameters)
        {
            _logger.LogInformation($"Getting filtered orders. Page: {parameters.Page}, PageSize: {parameters.PageSize}");
            var pagedResult = await _orderService.GetFilteredOrdersAsync(parameters);
            return Ok(pagedResult);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            // Only allow admin/employee or the customer who owns the order
            //if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            //{
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId) || userId != order.CustomerId)
                    return Forbid();
            //}

            return Ok(order);
        }

        [HttpGet("number/{orderNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber)
        {
            var order = await _orderService.GetOrderByOrderNumberAsync(orderNumber);
            if (order == null)
                return NotFound();

            // Only allow admin/employee or the customer who owns the order
            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId) || userId != order.CustomerId)
                    return Forbid();
            }

            return Ok(order);
        }

        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrders(Guid customerId)
        {
            // Only allow admin/employee or the customer themselves
            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId) || userId != customerId)
                    return Forbid();
            }

            var orders = await _orderService.GetCustomerOrdersAsync(customerId);
            return Ok(orders);
        }

        [HttpGet("status/{status}")]
        //[Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByStatus(OrderStatus status)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(orders);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(createOrderDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}/status")]
        //[Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] string status)
        {
            _logger.LogInformation($"Updating order status: Order ID = {id}, Status = {status}");
            
            // Check if order exists (this is now a redundant check but keeps the API behavior consistent)
            if (!await _orderService.ExistsAsync(id))
            {
                _logger.LogWarning($"Order with ID {id} not found");
                return NotFound("Order not found");
            }
            
            // Parse the string to OrderStatus enum
            if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                _logger.LogWarning($"Invalid status value received: {status}");
                string validValues = string.Join(", ", Enum.GetNames(typeof(OrderStatus)));
                return BadRequest($"Invalid order status. Valid values are: {validValues}");
            }
            
            // Call the service method to update the status
            var success = await _orderService.UpdateOrderStatusAsync(id, orderStatus, null);
            
            if (success)
            {
                _logger.LogInformation($"Successfully updated order {id} status to {orderStatus}");
                return Ok(new { Message = $"Order status updated to {orderStatus}" });
            }
            else
            {
                // Even though we checked existence above, something could have gone wrong in the update itself
                _logger.LogWarning($"Failed to update order status for order ID: {id}");
                return BadRequest("Unable to update order status. Please try again later.");
            }
        }

        [HttpPost("from-cart")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> CreateOrderFromCart([FromBody] CreateOrderFromCartDto createOrderDto)
        {
            try
            {
                _logger.LogInformation($"Creating order from cart {createOrderDto.CartId}");
                var order = await _orderService.CreateOrderFromCartAsync(createOrderDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order from cart: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the order");
            }
        }
    }
}