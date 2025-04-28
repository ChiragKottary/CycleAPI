using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
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
        private readonly IStockMovementRepository _stockMovementRepository;

        public StockController(
            IStockRepository stockRepository, 
            ICycleRepository cycleRepository,
            IStockMovementRepository stockMovementRepository)
        {
            this.stockRepository = stockRepository;
            this.cycleRepository = cycleRepository;
            _stockMovementRepository = stockMovementRepository;
        }

        [HttpGet("cycle/{cycleId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> GetCycleStock(Guid cycleId)
        {
            var cycle = await cycleRepository.GetByIdAsync(cycleId);
            if (cycle == null)
            {
                return NotFound("Cycle not found");
            }

            var stockLevel = await _stockMovementRepository.GetCurrentStockLevelAsync(cycleId);
            return Ok(new { cycleId, stockLevel });
        }

        [HttpGet("movements")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<StockMovement>>> GetStockMovements([FromQuery] StockMovementQueryParameters parameters)
        {
            var (movements, totalCount) = await _stockMovementRepository.GetFilteredAsync(parameters);
            
            var pagedResult = new PagedResult<StockMovement>
            {
                Items = movements,
                TotalItems = totalCount,
                PageNumber = parameters.Page,
                PageSize = parameters.PageSize
            };

            return Ok(pagedResult);
        }

        [HttpGet("cycle/{cycleId:guid}/movements")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<StockMovement>>> GetCycleStockMovements(Guid cycleId)
        {
            var cycle = await cycleRepository.GetByIdAsync(cycleId);
            if (cycle == null)
            {
                return NotFound("Cycle not found");
            }

            var movements = await _stockMovementRepository.GetByCycleIdAsync(cycleId);
            return Ok(movements);
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
