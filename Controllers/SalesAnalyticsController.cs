using CycleAPI.Models.DTO;
using CycleAPI.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CycleAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class SalesAnalyticsController : ControllerBase
    {
        private readonly ISalesAnalyticsRepository _salesAnalyticsRepository;

        public SalesAnalyticsController(ISalesAnalyticsRepository salesAnalyticsRepository)
        {
            _salesAnalyticsRepository = salesAnalyticsRepository;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyAnalytics([FromQuery] DateTime? date)
        {
            var targetDate = date ?? DateTime.UtcNow;
            var analytics = await _salesAnalyticsRepository.GetDailyAnalyticsAsync(targetDate);
            
            if (analytics == null)
            {
                analytics = await _salesAnalyticsRepository.UpdateDailyAnalyticsAsync(targetDate);
            }

            return Ok(analytics);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetAnalyticsSummary([FromQuery] SalesAnalyticsFilterDto filter)
        {
            var summary = await _salesAnalyticsRepository.GetAnalyticsSummaryAsync(filter);
            return Ok(summary);
        }

        [HttpGet("top-cycles")]
        public async Task<IActionResult> GetTopSellingCycles([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int top = 5)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            
            var topCycles = await _salesAnalyticsRepository.GetTopSellingCyclesAsync(start, end, top);
            return Ok(topCycles);
        }

        [HttpGet("top-brands")]
        public async Task<IActionResult> GetTopSellingBrands([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int top = 5)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            
            var topBrands = await _salesAnalyticsRepository.GetTopSellingBrandsAsync(start, end, top);
            return Ok(topBrands);
        }

        [HttpGet("period")]
        public async Task<IActionResult> GetAnalyticsForPeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var analytics = await _salesAnalyticsRepository.GetAnalyticsForPeriodAsync(startDate, endDate);
            return Ok(analytics);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAnalytics([FromQuery] DateTime? date)
        {
            var targetDate = date ?? DateTime.UtcNow;
            var analytics = await _salesAnalyticsRepository.UpdateDailyAnalyticsAsync(targetDate);
            return Ok(analytics);
        }
    }
}