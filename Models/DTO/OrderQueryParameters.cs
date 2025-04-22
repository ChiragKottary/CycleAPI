using CycleAPI.Models.DTO.Common;
using CycleAPI.Models.Enums;

namespace CycleAPI.Models.DTO
{
    public class OrderQueryParameters : BaseQueryParameters
    {
            public Guid? CustomerId { get; set; }
    public string? OrderNumber { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? CustomerName { get; set; }
    }
}