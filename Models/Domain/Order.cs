using CycleAPI.Models.Enums;

namespace CycleAPI.Models.Domain
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public  Customer Customer { get; set; }
        public required string OrderNumber { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public required string ShippingAddress { get; set; }
        public required string ShippingCity { get; set; }
        public required string ShippingState { get; set; }
        public required string ShippingPostalCode { get; set; }
        public string? Notes { get; set; }
        public Guid? ProcessedByUserId { get; set; }
        public User? ProcessedByUser { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}