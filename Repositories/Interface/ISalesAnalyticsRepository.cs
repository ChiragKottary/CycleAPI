using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO;

namespace CycleAPI.Repositories.Interface
{
    public interface ISalesAnalyticsRepository
    {
        Task<SalesAnalytics> GetDailyAnalyticsAsync(DateTime date);
        Task<IEnumerable<SalesAnalytics>> GetAnalyticsForPeriodAsync(DateTime startDate, DateTime endDate);
        Task<SalesAnalyticsSummaryDto> GetAnalyticsSummaryAsync(SalesAnalyticsFilterDto filter);
        Task<SalesAnalytics> UpdateDailyAnalyticsAsync(DateTime date);
        Task<IEnumerable<TopSellingItemDto>> GetTopSellingCyclesAsync(DateTime startDate, DateTime endDate, int top = 5);
        Task<IEnumerable<TopSellingItemDto>> GetTopSellingBrandsAsync(DateTime startDate, DateTime endDate, int top = 5);
    }
}