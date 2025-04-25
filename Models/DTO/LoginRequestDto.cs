using System.ComponentModel.DataAnnotations;

namespace CycleAPI.Models.DTO
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
