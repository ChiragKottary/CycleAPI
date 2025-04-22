using CycleAPI.Models.Enums;

namespace CycleAPI.Models.DTO
{
    public class AddStocksRequestDto
    {
        public Guid CycleId { get; set; }
        public int Quantity { get; set; }
        public Guid UserId { get; set; }
        public MovementType MovementType { get; set; }
        public string Notes { get; set; }
    }
}
