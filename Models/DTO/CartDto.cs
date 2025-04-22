namespace CycleAPI.Models.DTO
{
    public class CartDto
    {
        public Guid CartId { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? SessionId { get; set; }
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new();
    }
}
