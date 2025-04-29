using CycleAPI.Data;
using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;
using CycleAPI.Models.Enums;
using CycleAPI.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace CycleAPI.Repositories.Implementation
{
    public class SalesAnalyticsRepository : ISalesAnalyticsRepository
    {
        private readonly ApplicationDbContext _context;

        public SalesAnalyticsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SalesAnalytics> GetDailyAnalyticsAsync(DateTime date)
        {
            return await _context.SalesAnalytics
                .Include(x => x.TopSellingCycle)
                .Include(x => x.TopSellingBrand)
                .FirstOrDefaultAsync(x => x.Date.Date == date.Date);
        }

        public async Task<IEnumerable<SalesAnalytics>> GetAnalyticsForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.SalesAnalytics
                .Include(x => x.TopSellingCycle)
                .Include(x => x.TopSellingBrand)
                .Where(x => x.Date.Date >= startDate.Date && x.Date.Date <= endDate.Date)
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<SalesAnalyticsSummaryDto> GetAnalyticsSummaryAsync(SalesAnalyticsFilterDto filter)
        {
            var startDate = filter.StartDate ?? DateTime.UtcNow.AddDays(-30);
            var endDate = filter.EndDate ?? DateTime.UtcNow;

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Cycle)
                .ThenInclude(c => c.Brand)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Where(o => o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            var summary = new SalesAnalyticsSummaryDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                TotalProfit = CalculateTotalProfit(orders),
            };

            summary.ProfitMargin = summary.TotalRevenue > 0 
                ? (summary.TotalProfit / summary.TotalRevenue) * 100 
                : 0;

            // Get top selling cycles
            var topCycles = orders.SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Cycle)
                .Select(g => new TopSellingItemDto
                {
                    Name = g.Key.ModelName,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Subtotal)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(5)
                .ToList();

            // Get top selling brands
            var topBrands = orders.SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Cycle.Brand)
                .Select(g => new TopSellingItemDto
                {
                    Name = g.Key.BrandName,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Subtotal)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(5)
                .ToList();

            summary.TopSellingCycles = topCycles;
            summary.TopSellingBrands = topBrands;

            return summary;
        }

        public async Task<SalesAnalytics> UpdateDailyAnalyticsAsync(DateTime date)
        {
            // Ensure incoming date is in UTC
            date = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            
            var analytics = await _context.SalesAnalytics
                .FirstOrDefaultAsync(x => x.Date.Date == date.Date)
                ?? new SalesAnalytics { Date = date };

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Cycle)
                .Where(o => o.OrderDate.Date == date.Date)
                .Where(o => o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            // Calculate daily metrics
            analytics.DailyRevenue = orders.Sum(o => o.TotalAmount);
            analytics.TotalOrders = orders.Count;
            analytics.TotalUnitsSold = orders.Sum(o => o.OrderItems.Sum(oi => oi.Quantity));
            analytics.AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0;
            analytics.GrossProfit = CalculateTotalProfit(orders);
            analytics.NetProfit = analytics.GrossProfit; // Add overhead calculations if needed
            analytics.ProfitMargin = analytics.DailyRevenue > 0 
                ? (analytics.NetProfit / analytics.DailyRevenue) * 100 
                : 0;

            // Get top selling cycle for the day
            var topCycle = orders.SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.CycleId)
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .FirstOrDefault();

            if (topCycle != null)
            {
                analytics.TopSellingCycleId = topCycle.Key;
            }

            // Get top selling brand for the day
            var topBrand = orders.SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Cycle.BrandId)
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .FirstOrDefault();

            if (topBrand != null)
            {
                analytics.TopSellingBrandId = topBrand.Key;
            }

            // Calculate monthly metrics - ensure dates are UTC
            var monthStart = DateTime.SpecifyKind(new DateTime(date.Year, date.Month, 1), DateTimeKind.Utc);
            var monthEnd = DateTime.SpecifyKind(monthStart.AddMonths(1).AddDays(-1), DateTimeKind.Utc);
            
            analytics.MonthlyRevenue = await _context.Orders
                .Where(o => o.OrderDate.Date >= monthStart && o.OrderDate.Date <= monthEnd)
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);

            // Calculate yearly metrics - ensure dates are UTC
            var yearStart = DateTime.SpecifyKind(new DateTime(date.Year, 1, 1), DateTimeKind.Utc);
            var yearEnd = DateTime.SpecifyKind(new DateTime(date.Year, 12, 31), DateTimeKind.Utc);
            
            analytics.YearlyRevenue = await _context.Orders
                .Where(o => o.OrderDate.Date >= yearStart && o.OrderDate.Date <= yearEnd)
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);

            if (analytics.AnalyticsId == Guid.Empty)
            {
                analytics.CreatedAt = DateTime.UtcNow;
                await _context.SalesAnalytics.AddAsync(analytics);
            }

            analytics.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return analytics;
        }

        public async Task<IEnumerable<TopSellingItemDto>> GetTopSellingCyclesAsync(DateTime startDate, DateTime endDate, int top = 5)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Cycle)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
                .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
                .GroupBy(oi => new { oi.CycleId, oi.Cycle.ModelName })
                .Select(g => new TopSellingItemDto
                {
                    Name = g.Key.ModelName,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Subtotal)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(top)
                .ToListAsync();
        }

        public async Task<IEnumerable<TopSellingItemDto>> GetTopSellingBrandsAsync(DateTime startDate, DateTime endDate, int top = 5)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Cycle)
                .ThenInclude(c => c.Brand)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
                .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
                .GroupBy(oi => new { oi.Cycle.BrandId, oi.Cycle.Brand.BrandName })
                .Select(g => new TopSellingItemDto
                {
                    Name = g.Key.BrandName,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Subtotal)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(top)
                .ToListAsync();
        }

        private decimal CalculateTotalProfit(IEnumerable<Order> orders)
        {
            return orders.SelectMany(o => o.OrderItems)
                .Sum(oi => (oi.UnitPrice - oi.Cycle.CostPrice) * oi.Quantity);
        }
    }
}