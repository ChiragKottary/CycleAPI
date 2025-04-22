using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Models.DTO
{
    public class OrderItemQueryParameters : BaseQueryParameters
    {
        public Guid? OrderId { get; set; }
        public Guid? CycleId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
    }
}