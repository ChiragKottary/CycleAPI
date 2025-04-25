using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.Enums;
using CycleAPI.Repositories.Implementation;
using CycleAPI.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CycleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockRepository stockRepository;
        private readonly ICycleRepository cycleRepository;

        public StockController(IStockRepository stockRepository, ICycleRepository cycleRepository)
        {
            this.stockRepository = stockRepository;
            this.cycleRepository = cycleRepository;
        }

        [HttpPost]
        [Route("CycleStocksAdjustment")]
        public async Task<IActionResult> CycleStocksAdjustment([FromBody] AddStocksRequestDto request)
        {
            // First get the cycle to update its stock
            var cycle = await cycleRepository.GetByIdAsync(request.CycleId);
            if (cycle == null)
            {
                return NotFound("Cycle not found");
            }

            // Calculate new stock quantity based on movement type
            var previousQuantity = cycle.StockQuantity;
            switch (request.MovementType)
            {
                case MovementType.IN:
                    cycle.StockQuantity += request.Quantity;
                    break;
                case MovementType.OUT:
                    if (cycle.StockQuantity < request.Quantity)
                    {
                        return BadRequest($"Insufficient stock. Available: {cycle.StockQuantity}");
                    }
                    cycle.StockQuantity -= request.Quantity;
                    break;
                case MovementType.ADJUSTMENT:
                    cycle.StockQuantity = request.Quantity; // Direct adjustment
                    break;
                default:
                    return BadRequest("Invalid movement type");
            }

            // Create stock movement record with all required details
            var stockRecord = new StockMovement
            {
                MovementId = Guid.NewGuid(),
                CycleId = request.CycleId,
                Quantity = request.MovementType == MovementType.ADJUSTMENT ? 
                    request.Quantity - previousQuantity : request.Quantity,
                MovementType = request.MovementType,
                UserId = request.UserId,
                Notes = request.Notes ?? $"{request.MovementType} movement for {cycle.ModelName}",
                MovementDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Cycle = cycle
            };

            try
            {
                // Update cycle
                cycle.UpdatedAt = DateTime.UtcNow;
                await cycleRepository.UpdateAsync(cycle);

                // Add stock movement
                stockRecord = await stockRepository.AddMovementAsync(stockRecord);

                // Return full details including cycle information
                return Ok(new
                {
                    Movement = new
                    {
                        stockRecord.MovementId,
                        stockRecord.CycleId,
                        stockRecord.Quantity,
                        stockRecord.MovementType,
                        stockRecord.UserId,
                        stockRecord.Notes,
                        stockRecord.MovementDate,
                        stockRecord.UpdatedAt
                    },
                    Cycle = new
                    {
                        cycle.CycleId,
                        cycle.ModelName,
                        cycle.StockQuantity,
                        cycle.Price,
                        cycle.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating stock: {ex.Message}");
            }
        }
    }
}
