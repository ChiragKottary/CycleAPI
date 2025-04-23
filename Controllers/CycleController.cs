using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Repositories.Interface;
using CycleAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CycleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CycleController : ControllerBase
    {
        private readonly ICycleService _cycleService;
        private readonly ILogger<CycleController> _logger;

        public CycleController(ICycleService cycleService, ILogger<CycleController> logger)
        {
            _cycleService = cycleService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<Cycle>>> GetFilteredCycles([FromQuery] CycleQueryParameters parameters)
        {
            _logger.LogInformation($"Getting filtered cycles. Page: {parameters.Page}, PageSize: {parameters.PageSize}");
            var pagedResult = await _cycleService.GetFilteredCyclesAsync(parameters);
            return Ok(pagedResult);
        }

        [HttpGet("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var cycle = await _cycleService.GetCycleByIdAsync(id);
            if (cycle == null)
                return NotFound();

            return Ok(cycle);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCycle([FromBody] CreateCycleRequestDto cycle)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var cycle1 = new Cycle
            {
                ModelName = cycle.ModelName,
                TypeId = cycle.TypeId,
                BrandId = cycle.BrandId,
                Price = cycle.Price,
                Description = cycle.Description,
                ImageUrl = cycle.ImageUrl,
            };

            var createdCycle = await _cycleService.CreateCycleAsync(cycle1);
            return CreatedAtAction(nameof(GetById), new { id = createdCycle.CycleId }, createdCycle);
        }

        [HttpPut("{id:Guid}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCycle(Guid id, [FromBody] Cycle cycle)
        {
            if (id != cycle.CycleId)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedCycle = await _cycleService.UpdateCycleAsync(cycle);
            if (updatedCycle == null)
                return NotFound();

            return Ok(updatedCycle);
        }

        [HttpDelete("{id:Guid}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCycle(Guid id)
        {
            var success = await _cycleService.DeleteCycleAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
