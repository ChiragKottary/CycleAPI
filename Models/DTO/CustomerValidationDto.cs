using System.ComponentModel.DataAnnotations;

namespace CycleAPI.Models.DTO
{
    public class CustomerValidationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string PostalCode { get; set; }
    }
}
