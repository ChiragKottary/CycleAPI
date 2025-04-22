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
        public int subtotal { get; set; }
        public decimal Subtotal { get; internal set; }
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
