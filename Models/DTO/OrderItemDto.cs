using CycleAPI.Models.Domain;

namespace CycleAPI.Models.DTO
{
    public class OrderItemDto
    {
        public Guid OrderItemId { get; set; }
        public Guid OrderId { get; set; }
        public Guid CycleId { get; set; }
        public string CycleName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Order? Order { get; set; }
        public Cycle? Cycle { get; set; }
    }
}