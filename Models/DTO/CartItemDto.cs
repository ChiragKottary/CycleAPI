using CycleAPI.Models.Domain;

namespace CycleAPI.Models.DTO
{
    public class CartItemDto
    {
        public Guid CartItemId { get; set; }
        public Guid CartId { get; set; }
        public Guid CycleId { get; set; }
        public string? CycleName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Subtotal { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Cart? Cart { get; set; }
        public Cycle? Cycle { get; set; }
    }
}
