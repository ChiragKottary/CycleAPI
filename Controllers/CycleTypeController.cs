using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CycleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CycleTypeController : ControllerBase
    {
        private readonly ICycleTypeService _cycleTypeService;
        private readonly ILogger<CycleTypeController> _logger;

        public CycleTypeController(ICycleTypeService cycleTypeService, ILogger<CycleTypeController> logger)
        {
            _cycleTypeService = cycleTypeService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CycleType>>> GetFilteredTypes([FromQuery] CycleTypeQueryParameters parameters)
        {
            _logger.LogInformation($"Getting filtered cycle types. Page: {parameters.Page}, PageSize: {parameters.PageSize}");
            var pagedResult = await _cycleTypeService.GetFilteredTypesAsync(parameters);
            return Ok(pagedResult);
        }

        [HttpGet("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var cycleType = await _cycleTypeService.GetByIdAsync(id);
            if (cycleType == null)
            {
                return NotFound();
            }
            return Ok(cycleType);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCycleTypeRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cycleType = new CycleType
            {
                TypeName = request.TypeName,
                Description = request.Description
            };

            var createdType = await _cycleTypeService.CreateAsync(cycleType);
            return CreatedAtAction(nameof(GetById), new { id = createdType.TypeId }, createdType);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] CycleType cycleType)
        {
            if (id != cycleType.TypeId)
            {
                return BadRequest("ID mismatch between route and body");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedType = await _cycleTypeService.UpdateAsync(cycleType);
            if (updatedType == null)
            {
                return NotFound();
            }
            return Ok(updatedType);
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var success = await _cycleTypeService.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
