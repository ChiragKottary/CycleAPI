using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CycleAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CartDto>>> GetFilteredCarts([FromQuery] CartQueryParameters parameters)
        {
            _logger.LogInformation($"Getting filtered carts. Page: {parameters.Page}, PageSize: {parameters.PageSize}");
            var pagedResult = await _cartService.GetFilteredCartsAsync(parameters);
            return Ok(pagedResult);
        }

        [HttpGet("{id:guid}")]
        //[Authorize(Roles = "Admin,Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> GetCartById(Guid id)
        {
            var cart = await _cartService.GetCartByIdAsync(id);
            if (cart == null)
            {
                return NotFound("Cart not found");
            }
            return Ok(cart);
        }

        [HttpPost("{cartId:guid}/items")]
        //[Authorize(Roles = "Admin,Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CartItemDto>> AddCartItem(Guid cartId, [FromBody] AddCartItemDto request)
        {
            try
            {
                var cartItem = await _cartService.AddItemToCartAsync(cartId, request);
                return Ok(cartItem);
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

        [HttpPut("items/{cartItemId:guid}")]
        //[Authorize(Roles = "Admin,Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CartItemDto>> UpdateCartItem(Guid cartItemId, [FromBody] UpdateCartItemDto request)
        {
            try
            {
                var cartItem = await _cartService.UpdateCartItemQuantityAsync(cartItemId, request);
                return Ok(cartItem);
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

        [HttpDelete("items/{cartItemId:guid}")]
        //[Authorize(Roles = "Admin,Customer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveCartItem(Guid cartItemId)
        {
            try
            {
                await _cartService.RemoveItemFromCartAsync(cartItemId);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound("Cart item not found");
            }
        }

        [HttpDelete("{cartId:guid}")]
        //[Authorize(Roles = "Admin,Customer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClearCart(Guid cartId)
        {
            try
            {
                await _cartService.ClearCartAsync(cartId);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound("Cart not found");
            }
        }

        [HttpGet("{cartId:guid}/total")]
        //[Authorize(Roles = "Admin,Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<decimal>> GetCartTotal(Guid cartId)
        {
            var total = await _cartService.CalculateCartTotalAsync(cartId);
            return Ok(total);
        }
    }
}