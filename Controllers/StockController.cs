using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.Enums;
using CycleAPI.Repositories.Implementation;
using CycleAPI.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CycleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockRepository stockRepository;
        private readonly ICycleRepository cycleRepository;

        public StockController(IStockRepository stockRepository,ICycleRepository cycleRepository)
        {
            this.stockRepository = stockRepository;
            this.cycleRepository = cycleRepository;
        }

        [HttpPost]
        [Route("CycleStocksAdjustment")]
        public async Task<IActionResult> CycleStocksAdjustment([FromBody] AddStocksRequestDto request)
        {
            var StockRecord = new StockMovement
            {
                CycleId = request.CycleId,
                Quantity = request.Quantity,
                MovementType = request.MovementType,
                UserId = request.UserId,
                Notes = request.Notes,
            };

             StockRecord = await stockRepository.AddMovementAsync(StockRecord);

            var res = new StockMovement
            {
                MovementId = StockRecord.MovementId,
                CycleId = StockRecord.CycleId,
                Quantity = StockRecord.Quantity,
                MovementType = StockRecord.MovementType,
                ReferenceId = StockRecord.ReferenceId,
                UserId = StockRecord.UserId,
                Notes = StockRecord.Notes,
                MovementDate = StockRecord.MovementDate,
                Cycle = await cycleRepository.GetByIdAsync(StockRecord.CycleId)
            };

            return Ok(res);
        }
    }
}
