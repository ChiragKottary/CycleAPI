using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.Enums;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace CycleAPI.Repositories.Implementation
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDbContext _context;

        public StockRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StockMovement> AddMovementAsync(StockMovement stockMovement)
        {
            stockMovement.MovementDate = DateTime.UtcNow;
            stockMovement.UpdatedAt = DateTime.UtcNow;
            
            // Ensure cycle is loaded
            if (stockMovement.Cycle == null)
            {
                var cycle = await _context.Cycles
                    .FirstOrDefaultAsync(c => c.CycleId == stockMovement.CycleId);
                if (cycle == null)
                {
                    throw new InvalidOperationException($"Cycle with ID {stockMovement.CycleId} not found");
                }
                stockMovement.Cycle = cycle;
            }

            await _context.StockMovement.AddAsync(stockMovement);
            await _context.SaveChangesAsync();

            // Reload the movement with all relationships
            return await _context.StockMovement
                .Include(sm => sm.Cycle)
                .Include(sm => sm.User)
                .FirstOrDefaultAsync(sm => sm.MovementId == stockMovement.MovementId);
        }

        public async Task<IEnumerable<StockMovement>> GetByCycleIdAsync(Guid cycleId)
        {
            return await _context.StockMovement
                .Include(sm => sm.Cycle)
                .Include(sm => sm.User)
                .Where(sm => sm.CycleId == cycleId)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetByTypeAsync(MovementType movementType)
        {
            return await _context.StockMovement
                .Include(sm => sm.Cycle)
                .Include(sm => sm.User)
                .Where(sm => sm.MovementType == movementType)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }
    }
}
