namespace CycleAPI.Models.DTO
{
    public class CustomerStatisticsDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? FirstOrderDate { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public string MostPurchasedBrand { get; set; }
        public string MostPurchasedCycleType { get; set; }
    }
}
