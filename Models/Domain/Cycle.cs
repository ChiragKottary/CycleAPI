namespace CycleAPI.Models.Domain
{
    public class Cycle
    {
        public Guid CycleId { get; set; }
        public string? ModelName { get; set; }
        public Guid BrandId { get; set; }
        public Guid TypeId { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int WarrantyMonths { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Brand? Brand { get; set; }
        public CycleType? CycleType { get; set; }
        public ICollection<CartItem>? CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem>? OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<StockMovement>? StockMovements { get; set; } = new List<StockMovement>();
    }
}
