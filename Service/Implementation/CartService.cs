using CycleAPI.Models.DTO;
using CycleAPI.Models.Domain;
using CycleAPI.Repositories.Interface;
using CycleAPI.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Service.Implementation
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CartDto?> GetActiveCartAsync(Guid customerId)
        {
            var cart = await _unitOfWork.Carts.GetActiveByCustomerIdAsync(customerId);
            if (cart == null)
            {
                return null;
            }

            await LogCartActivity(cart.CartId, customerId, null, "VIEW", null, null);
            return await MapToCartDto(cart);
        }

        public async Task<CartDto> CreateCartAsync(Guid customerId, string? sessionId = null)
        {
            var cart = await _unitOfWork.Carts.CreateCartAsync(customerId, sessionId ?? string.Empty);
            await LogCartActivity(cart.CartId, customerId, null, "CREATE", null, null);
            return await MapToCartDto(cart);
        }

        public async Task<CartDto?> GetCartByIdAsync(Guid cartId)
        {
            var cart = await _unitOfWork.Carts.GetByIdAsync(cartId);
            if (cart == null)
            {
                return null;
            }

            if (await IsCartExpiredAsync(cartId))
            {
                await CleanupExpiredCartsAsync();
                return null;
            }

            await LogCartActivity(cartId, cart.CustomerId, null, "VIEW", null, null);
            return await MapToCartDto(cart);
        }

        public async Task<IEnumerable<CartDto>> GetAllActiveCartsAsync()
        {
            var carts = await _unitOfWork.Carts.GetAllActiveAsync();
            var cartDtos = new List<CartDto>();

            foreach (var cart in carts)
            {
                cartDtos.Add(await MapToCartDto(cart));
            }

            return cartDtos;
        }

        public async Task<IEnumerable<CartDto>> SearchCartsByCustomerNameAsync(string name)
        {
            var carts = await _unitOfWork.Carts.SearchByCustomerNameAsync(name);
            var cartDtos = new List<CartDto>();

            foreach (var cart in carts)
            {
                cartDtos.Add(await MapToCartDto(cart));
            }

            return cartDtos;
        }

        public async Task<CartItemDto> AddItemToCartAsync(Guid cartId, AddCartItemDto addCartItemDto)
        {
            if (cartId == Guid.Empty)
                throw new ArgumentException("Invalid cart ID");
            if (addCartItemDto == null)
                throw new ArgumentException("Cart item details are required");
            if (addCartItemDto.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var cycle = await _unitOfWork.Cycles.GetByIdAsync(addCartItemDto.CycleId);
                if (cycle == null)
                {
                    throw new ArgumentException("Cycle not found");
                }

                if (cycle.StockQuantity < addCartItemDto.Quantity)
                {
                    throw new InvalidOperationException($"Not enough stock available. Currently available: {cycle.StockQuantity}");
                }

                var cart = await _unitOfWork.Carts.GetByIdAsync(cartId);
                if (cart == null)
                {
                    throw new ArgumentException("Cart not found");
                }

                var existingItem = await _unitOfWork.CartItems.GetByCartAndCycleAsync(cartId, addCartItemDto.CycleId);
                int previousQuantity = existingItem?.Quantity ?? 0;

                var cartItem = await _unitOfWork.CartItems.AddAsync(cartId, addCartItemDto.CycleId, addCartItemDto.Quantity);
                
                await LogCartActivity(cartId, cart.CustomerId, addCartItemDto.CycleId, "ADD", 
                    addCartItemDto.Quantity, previousQuantity);

                await _unitOfWork.CommitAsync();
                return MapToCartItemDto(cartItem, cycle);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<CartItemDto> UpdateCartItemQuantityAsync(Guid cartItemId, UpdateCartItemDto updateCartItemDto)
        {
            if (updateCartItemDto.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemId);
                if (cartItem == null)
                {
                    throw new ArgumentException("Cart item not found");
                }

                var cycle = await _unitOfWork.Cycles.GetByIdAsync(cartItem.CycleId);
                if (cycle == null)
                {
                    throw new ArgumentException("Cycle not found");
                }

                var cart = await _unitOfWork.Carts.GetByIdAsync(cartItem.CartId);
                if (cart == null)
                {
                    throw new ArgumentException("Cart not found");
                }

                // Calculate the additional quantity needed
                int quantityDifference = updateCartItemDto.Quantity - cartItem.Quantity;
                if (quantityDifference > 0 && cycle.StockQuantity < quantityDifference)
                {
                    throw new InvalidOperationException($"Not enough stock available. Currently available: {cycle.StockQuantity}");
                }

                int previousQuantity = cartItem.Quantity;
                var updatedCartItem = await _unitOfWork.CartItems.UpdateAsync(cartItemId, updateCartItemDto.Quantity);
                if (updatedCartItem == null)
                {
                    throw new InvalidOperationException("Failed to update cart item");
                }

                await LogCartActivity(cart.CartId, cart.CustomerId, updatedCartItem.CycleId, "UPDATE",
                    updateCartItemDto.Quantity, previousQuantity);

                await _unitOfWork.CommitAsync();
                return MapToCartItemDto(updatedCartItem, cycle);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task RemoveItemFromCartAsync(Guid cartItemId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemId);
                if (cartItem == null)
                {
                    throw new ArgumentException("Cart item not found");
                }

                var cart = await _unitOfWork.Carts.GetByIdAsync(cartItem.CartId);
                if (cart == null)
                {
                    throw new ArgumentException("Cart not found");
                }

                int previousQuantity = cartItem.Quantity;
                await _unitOfWork.CartItems.DeleteAsync(cartItemId);

                await LogCartActivity(cart.CartId, cart.CustomerId, cartItem.CycleId, "REMOVE",
                    0, previousQuantity);

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task ClearCartAsync(Guid cartId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var cart = await _unitOfWork.Carts.GetByIdAsync(cartId);
                if (cart == null)
                {
                    throw new ArgumentException("Cart not found");
                    
                }

                var cartItems = await _unitOfWork.CartItems.GetAllAsync(cartId);
                foreach (var item in cartItems)
                {
                    await _unitOfWork.CartItems.DeleteAsync(item.CartItemId);
                    await LogCartActivity(cartId, cart.CustomerId, item.CycleId, "REMOVE",
                        0, item.Quantity);
                }

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<decimal> CalculateCartTotalAsync(Guid cartId)
        {
            var cartItems = await _unitOfWork.CartItems.GetAllAsync(cartId);
            decimal total = 0;

            foreach (var item in cartItems)
            {
                var cycle = await _unitOfWork.Cycles.GetByIdAsync(item.CycleId);
                if (cycle != null)
                {
                    total += cycle.Price * item.Quantity;
                }
            }

            return total;
        }

        public async Task<bool> IsCartExpiredAsync(Guid cartId)
        {
            var cart = await _unitOfWork.Carts.GetByIdAsync(cartId);
            if (cart == null) return true;

            // Check if cart hasn't been updated in 24 hours
            var expirationTime = DateTime.UtcNow.AddHours(-24);
            return cart.UpdatedAt < expirationTime;
        }

        public async Task CleanupExpiredCartsAsync()
        {
            var activeCarts = await _unitOfWork.Carts.GetAllActiveAsync();
            foreach (var cart in activeCarts)
            {
                if (await IsCartExpiredAsync(cart.CartId))
                {
                    await ClearCartAsync(cart.CartId);
                    cart.IsActive = false;
                    await _unitOfWork.Carts.UpdateAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                    await LogCartActivity(cart.CartId, cart.CustomerId, null, "EXPIRE", null, null);
                }
            }
        }

        public async Task<PagedResult<CartDto>> GetFilteredCartsAsync(CartQueryParameters parameters)
        {
            var (carts, totalCount) = await _unitOfWork.Carts.GetFilteredAsync(parameters);
            
            var cartDtos = carts.Select(cart => new CartDto
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                CustomerName = cart.Customer != null ? $"{cart.Customer.FirstName} {cart.Customer.LastName}" : string.Empty,
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
                    UnitPrice = ci.Cycle.Price,
                    Quantity = ci.Quantity,
                    TotalPrice = ci.Cycle.Price * ci.Quantity,
                    Subtotal = ci.Cycle.Price * ci.Quantity,
                    AddedAt = ci.AddedAt,
                    UpdatedAt = ci.UpdatedAt,
                    Cart = ci.Cart,
                    Cycle = ci.Cycle
                }).ToList() ?? new List<CartItemDto>()
            }).ToList();

            return new PagedResult<CartDto>
            {
                Items = cartDtos,
                TotalItems = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };
        }

        private async Task LogCartActivity(Guid cartId, Guid customerId, Guid? cycleId, string action, 
            int? quantity, int? previousQuantity)
        {
            var userId = GetCurrentUserId();
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

            var activityLog = new CartActivityLog
            {
                CartId = cartId,
                CustomerId = customerId,
                CycleId = cycleId,
                Action = action,
                Quantity = quantity,
                PreviousQuantity = previousQuantity,
                UserId = userId,
                IpAddress = ipAddress
            };
            await _unitOfWork.CartActivityLogs.AddAsync(activityLog);
        }

        private Guid? GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                return userId;
            }
            return null;
        }

        private async Task<CartDto> MapToCartDto(Cart cart)
        {
            var customer = await _unitOfWork.Customers.GetCustomerByIdAsync(cart.CustomerId);
            var cartItems = await _unitOfWork.CartItems.GetAllAsync(cart.CartId);
            var cartItemDtos = new List<CartItemDto>();

            foreach (var item in cartItems)
            {
                var cycle = await _unitOfWork.Cycles.GetByIdAsync(item.CycleId);
                if (cycle != null)
                {
                    cartItemDtos.Add(MapToCartItemDto(item, cycle));
                }
            }

            return new CartDto
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}".Trim() : string.Empty,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                IsActive = cart.IsActive,
                SessionId = cart.SessionId,
                Notes = cart.Notes,
                TotalAmount = cartItemDtos.Sum(item => item.TotalPrice),
                TotalItems = cartItemDtos.Sum(item => item.Quantity),
                CartItems = cartItemDtos,
                LastAccessedByUserId = cart.LastAccessedByUserId,
                LastAccessedByUser = cart.LastAccessedByUser,
                LastAccessedAt = cart.LastAccessedAt
            };
        }

        private CartItemDto MapToCartItemDto(CartItem cartItem, Cycle cycle)
        {
            decimal totalPrice = cycle.Price * cartItem.Quantity;
            return new CartItemDto
            {
                CartItemId = cartItem.CartItemId,
                CartId = cartItem.CartId,
                CycleId = cartItem.CycleId,
                CycleName = cycle.ModelName,
                CycleBrand = cycle.Brand?.BrandName,
                CycleType = cycle.CycleType?.TypeName,
                CycleDescription = cycle.Description,
                CycleImage = cycle.ImageUrl,
                UnitPrice = cycle.Price,
                Quantity = cartItem.Quantity,
                TotalPrice = totalPrice,
                Subtotal = totalPrice,
                AddedAt = cartItem.AddedAt,
                UpdatedAt = cartItem.UpdatedAt,
                Cart = cartItem.Cart,
                Cycle = cycle
            };
        }

        public Task ExistsAsync(Guid cartId)
        {
            return _unitOfWork.Carts.ExistsAsync(cartId);
        }
    }
}
