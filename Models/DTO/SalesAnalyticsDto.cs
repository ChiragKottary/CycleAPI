using System;

namespace CycleAPI.Models.DTO
{
    public class SalesAnalyticsDto
    {
        public DateTime Date { get; set; }
        public decimal DailyRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUnitsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public string? TopSellingCycleName { get; set; }
        public string? TopSellingBrandName { get; set; }
    }

    public class SalesAnalyticsSummaryDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public List<TopSellingItemDto> TopSellingCycles { get; set; } = new();
        public List<TopSellingItemDto> TopSellingBrands { get; set; } = new();
    }

    public class TopSellingItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SalesAnalyticsFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? BrandId { get; set; }
        public Guid? CycleId { get; set; }
        public string? TimeFrame { get; set; } // Daily, Weekly, Monthly, Yearly
    }
}