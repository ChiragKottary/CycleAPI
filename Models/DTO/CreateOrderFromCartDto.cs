using System.ComponentModel.DataAnnotations;

namespace CycleAPI.Models.DTO
{
    public class CreateOrderFromCartDto
    {
        [Required]
        public Guid CartId { get; set; }

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
}