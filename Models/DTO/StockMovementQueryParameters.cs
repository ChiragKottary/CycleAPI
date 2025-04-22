using CycleAPI.Models.Domain;
using CycleAPI.Models.DTO.Common;
using CycleAPI.Models.Enums;

namespace CycleAPI.Models.DTO
{
    public class StockMovementQueryParameters : BaseQueryParameters
    {
        public Guid? CycleId { get; set; }
        public Guid? UserId { get; set; }
        public MovementType? MovementType { get; set; }
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}