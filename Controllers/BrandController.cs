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
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;
        private readonly ILogger<BrandController> _logger;

        public BrandController(IBrandService brandService, ILogger<BrandController> logger)
        {
            _brandService = brandService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<Brand>>> GetFilteredBrands([FromQuery] BrandQueryParameters parameters)
        {
            _logger.LogInformation($"Getting filtered brands. Page: {parameters.Page}, PageSize: {parameters.PageSize}");
            var pagedResult = await _brandService.GetFilteredBrandsAsync(parameters);
            return Ok(pagedResult);
        }

        [HttpGet("{id:Guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return Ok(brand);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] Brand brand)
        {
            var createdBrand = await _brandService.CreateAsync(brand);
            return CreatedAtAction(nameof(GetById), new { id = createdBrand.BrandId }, createdBrand);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] Brand brand)
        {
            var updatedBrand = await _brandService.UpdateAsync(brand);
            if (updatedBrand == null)
            {
                return NotFound();
            }
            return Ok(updatedBrand);
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deletedBrand = await _brandService.DeleteAsync(id);
            if (deletedBrand == null)
            {
                return NotFound();
            }
            return Ok(deletedBrand);
        }
    }
}
