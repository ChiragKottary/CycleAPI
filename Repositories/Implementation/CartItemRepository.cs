using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace CycleAPI.Repositories.Implementation
{
    public class CartItemRepository : ICartItemRepository
    {
        private readonly ApplicationDbContext _context;

        public CartItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartItem?> GetByIdAsync(Guid cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Cycle)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
        }

        public async Task<IEnumerable<CartItem>> GetAllAsync(Guid cartId)
        {
            return await _context.CartItems
                .Include(ci => ci.Cycle)
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();
        }

        public async Task<CartItem?> GetByCartAndCycleAsync(Guid cartId, Guid cycleId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.CycleId == cycleId);
        }

        public async Task<CartItem> AddAsync(Guid cartId, Guid cycleId, int quantity)
        {
            var existingItem = await GetByCartAndCycleAsync(cartId, cycleId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                _context.Entry(existingItem).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return existingItem;
            }

            var cartItem = new CartItem
            {
                CartItemId = Guid.NewGuid(),
                CartId = cartId,
                CycleId = cycleId,
                Quantity = quantity,
                AddedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task<CartItem?> UpdateAsync(Guid cartItemId, int quantity)
        {
            var cartItem = await GetByIdAsync(cartItemId);
            if (cartItem == null)
                return null;

            cartItem.Quantity = quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            _context.Entry(cartItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task<bool> DeleteAsync(Guid cartItemId)
        {
            var cartItem = await GetByIdAsync(cartItemId);
            if (cartItem == null)
                return false;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid cartItemId)
        {
            return await _context.CartItems.AnyAsync(ci => ci.CartItemId == cartItemId);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }
    }
}
