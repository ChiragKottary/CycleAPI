using System.ComponentModel.DataAnnotations;

namespace CycleAPI.Models.DTO
{
    public class OrderCreateDto
    {
        [Required]
        public string ShippingAddress { get; set; }

        [Required]
        public string ShippingCity { get; set; }

        [Required]
        public string ShippingState { get; set; }

        [Required]
        public string ShippingPostalCode { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        public string Notes { get; set; }
    }
}
