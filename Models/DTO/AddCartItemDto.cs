using System.ComponentModel.DataAnnotations;

namespace CycleAPI.Models.DTO
{
    public class AddCartItemDto
    {
        [Required]
        public Guid CycleId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
