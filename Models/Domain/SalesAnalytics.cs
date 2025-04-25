using System;

namespace CycleAPI.Models.Domain
{
    public class SalesAnalytics
    {
        public Guid AnalyticsId { get; set; }
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
        public Guid? TopSellingCycleId { get; set; }
        public Cycle? TopSellingCycle { get; set; }
        public Guid? TopSellingBrandId { get; set; }
        public Brand? TopSellingBrand { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}