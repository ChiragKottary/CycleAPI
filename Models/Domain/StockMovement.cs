using CycleAPI.Models.Enums;

namespace CycleAPI.Models.Domain
{
    public class StockMovement
    {
        public Guid MovementId { get; set; }
        public Guid CycleId { get; set; }
        public int Quantity { get; set; }
        public MovementType MovementType { get; set; } // Expected values: "IN", "OUT", "ADJUSTMENT"
        public Guid? ReferenceId { get; set; }     // Nullable
        public Guid UserId { get; set; }
        public string Notes { get; set; }
        public DateTime MovementDate { get; set; }
        public  DateTime UpdatedAt { get; set; }
        // Navigation properties
        public Cycle Cycle { get; set; }
        public User User { get; set; }
    }
}
