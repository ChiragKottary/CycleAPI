using CycleAPI.Models.Domain;
using CycleAPI.Models.Enums;

namespace CycleAPI.Models.DTO
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public Guid? ProcessedByUserId { get; set; }
        public string? ProcessedByUserName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public Customer? Customer { get; set; }
        public User? ProcessedByUser { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}