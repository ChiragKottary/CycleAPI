using System.ComponentModel.DataAnnotations;

namespace CycleAPI.Models.DTO
{
    public class CreateOrderDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public List<CreateOrderItemDto> OrderItems { get; set; } = new();

        [Required]
        [MinLength(5)]
        [MaxLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string ShippingCity { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string ShippingState { get; set; } = string.Empty;

        [Required]
        [MinLength(5)]
        [MaxLength(20)]
        public string ShippingPostalCode { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }

    public class CreateOrderItemDto
    {
        [Required]
        public Guid CycleId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public string? Notes { get; set; }
    }
}