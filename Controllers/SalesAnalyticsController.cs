using CycleAPI.Models.DTO;
using CycleAPI.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace CycleAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class SalesAnalyticsController : ControllerBase
    {
        private readonly ISalesAnalyticsRepository _salesAnalyticsRepository;
        private const string DateFormat = "yyyy-MM-dd";

        public SalesAnalyticsController(ISalesAnalyticsRepository salesAnalyticsRepository)
        {
            _salesAnalyticsRepository = salesAnalyticsRepository;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyAnalytics([FromQuery] string date)
        {
            // Parse date in yyyy-MM-dd format
            DateTime targetDate;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParseExact(date, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate))
            {
                targetDate = DateTime.SpecifyKind(targetDate, DateTimeKind.Utc);
            }
            else
            {
                targetDate = DateTime.UtcNow.Date;
            }
            
            var analytics = await _salesAnalyticsRepository.GetDailyAnalyticsAsync(targetDate);
            
            if (analytics == null)
            {
                analytics = await _salesAnalyticsRepository.UpdateDailyAnalyticsAsync(targetDate);
            }

            return Ok(analytics);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetAnalyticsSummary([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] SalesAnalyticsFilterDto filter = null)
        {
            filter = filter ?? new SalesAnalyticsFilterDto();
            
            // Parse dates in yyyy-MM-dd format
            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParseExact(startDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedStartDate))
            {
                filter.StartDate = DateTime.SpecifyKind(parsedStartDate, DateTimeKind.Utc);
            }
            
            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParseExact(endDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEndDate))
            {
                filter.EndDate = DateTime.SpecifyKind(parsedEndDate, DateTimeKind.Utc);
            }
            
            var summary = await _salesAnalyticsRepository.GetAnalyticsSummaryAsync(filter);
            return Ok(summary);
        }

        [HttpGet("top-cycles")]
        public async Task<IActionResult> GetTopSellingCycles([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] int top = 5)
        {
            DateTime start = DateTime.UtcNow.AddDays(-30);
            DateTime end = DateTime.UtcNow;
            
            // Parse dates in yyyy-MM-dd format
            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime.TryParseExact(startDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out start);
                start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            }
            
            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime.TryParseExact(endDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out end);
                end = DateTime.SpecifyKind(end, DateTimeKind.Utc);
            }
            
            var topCycles = await _salesAnalyticsRepository.GetTopSellingCyclesAsync(start, end, top);
            return Ok(topCycles);
        }

        [HttpGet("top-brands")]
        public async Task<IActionResult> GetTopSellingBrands([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] int top = 5)
        {
            DateTime start = DateTime.UtcNow.AddDays(-30);
            DateTime end = DateTime.UtcNow;
            
            // Parse dates in yyyy-MM-dd format
            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime.TryParseExact(startDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out start);
                start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            }
            
            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime.TryParseExact(endDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out end);
                end = DateTime.SpecifyKind(end, DateTimeKind.Utc);
            }
            
            var topBrands = await _salesAnalyticsRepository.GetTopSellingBrandsAsync(start, end, top);
            return Ok(topBrands);
        }

        [HttpGet("period")]
        public async Task<IActionResult> GetAnalyticsForPeriod([FromQuery] string startDate, [FromQuery] string endDate)
        {
            // Parse and validate dates
            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return BadRequest("Both startDate and endDate are required in yyyy-MM-dd format");
            }
            
            if (!DateTime.TryParseExact(startDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var utcStartDate) ||
                !DateTime.TryParseExact(endDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var utcEndDate))
            {
                return BadRequest("Dates must be in yyyy-MM-dd format");
            }
            
            // Ensure dates are in UTC
            utcStartDate = DateTime.SpecifyKind(utcStartDate, DateTimeKind.Utc);
            utcEndDate = DateTime.SpecifyKind(utcEndDate, DateTimeKind.Utc);
            
            var analytics = await _salesAnalyticsRepository.GetAnalyticsForPeriodAsync(utcStartDate, utcEndDate);
            return Ok(analytics);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAnalytics([FromQuery] string date)
        {
            DateTime targetDate;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParseExact(date, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate))
            {
                targetDate = DateTime.SpecifyKind(targetDate, DateTimeKind.Utc);
            }
            else
            {
                targetDate = DateTime.UtcNow;
            }
            
            var analytics = await _salesAnalyticsRepository.UpdateDailyAnalyticsAsync(targetDate);
            return Ok(analytics);
        }

        #region Chart-specific Endpoints

        [HttpGet("chart/revenue-trend")]
        public async Task<IActionResult> GetRevenueChartData([FromQuery] string startDate, [FromQuery] string endDate)
        {
            // Parse and validate dates
            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return BadRequest("Both startDate and endDate are required in yyyy-MM-dd format");
            }

            if (!DateTime.TryParseExact(startDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var utcStartDate) ||
                !DateTime.TryParseExact(endDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var utcEndDate))
            {
                return BadRequest("Dates must be in yyyy-MM-dd format");
            }

            // Ensure dates are in UTC
            utcStartDate = DateTime.SpecifyKind(utcStartDate, DateTimeKind.Utc);
            utcEndDate = DateTime.SpecifyKind(utcEndDate, DateTimeKind.Utc);

            var analytics = await _salesAnalyticsRepository.GetAnalyticsForPeriodAsync(utcStartDate, utcEndDate);

            // Format data specifically for chart use
            var chartData = new
            {
                labels = analytics.Select(a => a.Date.ToString("MMM dd")).ToArray(),
                datasets = new[] 
                {
                    new {
                        label = "Daily Revenue",
                        data = analytics.Select(a => a.DailyRevenue).ToArray(),
                        borderColor = "#36a2eb",
                        backgroundColor = "rgba(54, 162, 235, 0.2)",
                        borderWidth = 2,
                        fill = true
                    },
                    new {
                        label = "Profit",
                        data = analytics.Select(a => a.NetProfit).ToArray(),
                        borderColor = "#ff6384",
                        backgroundColor = "rgba(255, 99, 132, 0.2)",
                        borderWidth = 2,
                        fill = true
                    }
                },
                type = "line"
            };

            return Ok(chartData);
        }

        [HttpGet("chart/top-cycles")]
        public async Task<IActionResult> GetTopCyclesChartData([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] int top = 5)
        {
            DateTime start = DateTime.UtcNow.AddDays(-30);
            DateTime end = DateTime.UtcNow;

            // Parse dates in yyyy-MM-dd format
            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime.TryParseExact(startDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out start);
                start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime.TryParseExact(endDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out end);
                end = DateTime.SpecifyKind(end, DateTimeKind.Utc);
            }

            var topCycles = await _salesAnalyticsRepository.GetTopSellingCyclesAsync(start, end, top);

            // Generate random colors for the chart
            var colors = GenerateRandomColors(topCycles.Count());
            var backgroundColors = colors.Select(c => $"rgba({c.R}, {c.G}, {c.B}, 0.2)").ToArray();
            var borderColors = colors.Select(c => $"rgb({c.R}, {c.G}, {c.B})").ToArray();

            // Format data for pie/bar charts
            var chartData = new
            {
                labels = topCycles.Select(c => c.Name).ToArray(),
                datasets = new[] 
                {
                    new {
                        label = "Units Sold",
                        data = topCycles.Select(c => c.UnitsSold).ToArray(),
                        backgroundColor = backgroundColors,
                        borderColor = borderColors,
                        borderWidth = 1
                    }
                },
                type = "pie" // can be "bar" as well
            };

            return Ok(chartData);
        }

        [HttpGet("chart/top-brands")]
        public async Task<IActionResult> GetTopBrandsChartData([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] int top = 5)
        {
            DateTime start = DateTime.UtcNow.AddDays(-30);
            DateTime end = DateTime.UtcNow;

            // Parse dates in yyyy-MM-dd format
            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime.TryParseExact(startDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out start);
                start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime.TryParseExact(endDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out end);
                end = DateTime.SpecifyKind(end, DateTimeKind.Utc);
            }

            var topBrands = await _salesAnalyticsRepository.GetTopSellingBrandsAsync(start, end, top);

            // Generate random colors for the chart
            var colors = GenerateRandomColors(topBrands.Count());
            var backgroundColors = colors.Select(c => $"rgba({c.R}, {c.G}, {c.B}, 0.2)").ToArray();
            var borderColors = colors.Select(c => $"rgb({c.R}, {c.G}, {c.B})").ToArray();

            // Format data for doughnut/radar charts
            var chartData = new
            {
                labels = topBrands.Select(b => b.Name).ToArray(),
                datasets = new[] 
                {
                    new {
                        label = "Revenue",
                        data = topBrands.Select(b => b.Revenue).ToArray(),
                        backgroundColor = backgroundColors,
                        borderColor = borderColors,
                        borderWidth = 1
                    }
                },
                type = "doughnut" // can be "radar" as well
            };

            return Ok(chartData);
        }

        [HttpGet("chart/sales-comparison")]
        public async Task<IActionResult> GetSalesComparisonChartData([FromQuery] string startDate, [FromQuery] string endDate)
        {
            // Parse and validate dates
            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return BadRequest("Both startDate and endDate are required in yyyy-MM-dd format");
            }

            if (!DateTime.TryParseExact(startDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var utcStartDate) ||
                !DateTime.TryParseExact(endDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var utcEndDate))
            {
                return BadRequest("Dates must be in yyyy-MM-dd format");
            }

            // Ensure dates are in UTC
            utcStartDate = DateTime.SpecifyKind(utcStartDate, DateTimeKind.Utc);
            utcEndDate = DateTime.SpecifyKind(utcEndDate, DateTimeKind.Utc);

            var analytics = await _salesAnalyticsRepository.GetAnalyticsForPeriodAsync(utcStartDate, utcEndDate);

            // Format data for bar chart comparing multiple metrics
            var chartData = new
            {
                labels = analytics.Select(a => a.Date.ToString("MMM dd")).ToArray(),
                datasets = new List<object> 
                {
                    new {
                        label = "Orders",
                        data = analytics.Select(a => a.TotalOrders).ToArray(),
                        backgroundColor = "rgba(54, 162, 235, 0.5)",
                        yAxisID = "y-orders"
                    },
                    new {
                        label = "Revenue",
                        data = analytics.Select(a => a.DailyRevenue).ToArray(),
                        backgroundColor = "rgba(255, 99, 132, 0.5)",
                        yAxisID = "y-revenue"
                    },
                    new {
                        label = "Units Sold",
                        data = analytics.Select(a => a.TotalUnitsSold).ToArray(),
                        backgroundColor = "rgba(75, 192, 192, 0.5)",
                        yAxisID = "y-units"
                    }
                },
                type = "bar",
                options = new
                {
                    scales = new
                    {
                        yAxes = new[] 
                        {
                            new { 
                                id = "y-orders", 
                                type = "linear",
                                position = "left",
                                display = true, 
                                title = new { display = true, text = "Orders" } 
                            },
                            new { 
                                id = "y-revenue", 
                                type = "linear",
                                position = "right",
                                display = true, 
                                title = new { display = true, text = "Revenue" } 
                            },
                            new { 
                                id = "y-units", 
                                type = "linear",
                                position = "right",
                                display = true, 
                                title = new { display = true, text = "Units Sold" } 
                            }
                        }
                    }
                }
            };

            return Ok(chartData);
        }

        [HttpGet("chart/summary")]
        public async Task<IActionResult> GetSummaryChartData([FromQuery] string startDate, [FromQuery] string endDate)
        {
            var filter = new SalesAnalyticsFilterDto();
            
            // Parse dates in yyyy-MM-dd format
            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParseExact(startDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedStartDate))
            {
                filter.StartDate = DateTime.SpecifyKind(parsedStartDate, DateTimeKind.Utc);
            }
            
            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParseExact(endDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEndDate))
            {
                filter.EndDate = DateTime.SpecifyKind(parsedEndDate, DateTimeKind.Utc);
            }
            
            var summary = await _salesAnalyticsRepository.GetAnalyticsSummaryAsync(filter);

            // Format data for summary gauges/charts
            var chartData = new
            {
                summaryStats = new List<object> 
                {
                    new { label = "Total Revenue", value = summary.TotalRevenue, color = "#36a2eb", format = "currency" },
                    new { label = "Orders", value = summary.TotalOrders, color = "#ff6384", format = "number" },
                    new { label = "Profit Margin", value = summary.ProfitMargin, color = "#4bc0c0", format = "percent" },
                    new { label = "Avg Order Value", value = summary.AverageOrderValue, color = "#ffcd56", format = "currency" },
                    new { label = "Total Profit", value = summary.TotalProfit, color = "#9966ff", format = "currency" }
                },
                topCycles = new
                {
                    labels = summary.TopSellingCycles.Select(c => c.Name).ToArray(),
                    data = summary.TopSellingCycles.Select(c => c.UnitsSold).ToArray(),
                    colors = new[] { "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF" }
                },
                topBrands = new
                {
                    labels = summary.TopSellingBrands.Select(b => b.Name).ToArray(),
                    data = summary.TopSellingBrands.Select(b => b.Revenue).ToArray(),
                    colors = new[] { "#36A2EB", "#FF6384", "#4BC0C0", "#FFCE56", "#9966FF" }
                },
                dateRange = new
                {
                    start = summary.StartDate.ToString(DateFormat),
                    end = summary.EndDate.ToString(DateFormat)
                }
            };

            return Ok(chartData);
        }

        #endregion

        #region Helper Methods

        private class RgbColor
        {
            public int R { get; set; }
            public int G { get; set; }
            public int B { get; set; }
        }

        private List<RgbColor> GenerateRandomColors(int count)
        {
            var colors = new List<RgbColor>();
            var random = new Random();

            // Predefined colors for common charts (up to 10)
            var predefinedColors = new List<RgbColor>
            {
                new RgbColor { R = 255, G = 99, B = 132 },  // Red
                new RgbColor { R = 54, G = 162, B = 235 },  // Blue
                new RgbColor { R = 255, G = 206, B = 86 },  // Yellow
                new RgbColor { R = 75, G = 192, B = 192 },  // Green
                new RgbColor { R = 153, G = 102, B = 255 }, // Purple
                new RgbColor { R = 255, G = 159, B = 64 },  // Orange
                new RgbColor { R = 199, G = 199, B = 199 }, // Gray
                new RgbColor { R = 83, G = 102, B = 255 },  // Indigo
                new RgbColor { R = 255, G = 99, B = 255 },  // Pink
                new RgbColor { R = 99, G = 255, B = 132 }   // Light Green
            };

            // Use predefined colors first
            for (int i = 0; i < Math.Min(count, predefinedColors.Count); i++)
            {
                colors.Add(predefinedColors[i]);
            }

            // If we need more colors than predefined, generate random ones
            for (int i = predefinedColors.Count; i < count; i++)
            {
                colors.Add(new RgbColor
                {
                    R = random.Next(0, 255),
                    G = random.Next(0, 255),
                    B = random.Next(0, 255)
                });
            }

            return colors;
        }

        #endregion
    }
}