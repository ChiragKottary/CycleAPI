
using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Models.DTO
{
    public class CartActivityLogQueryParameters : BaseQueryParameters
    {
        public Guid? CartId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? CycleId { get; set; }
        public Guid? UserId { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}