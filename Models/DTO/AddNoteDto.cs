using System.ComponentModel.DataAnnotations;

namespace CycleAPI.Models.DTO
{
    public class AddNoteDto
    {
        [Required]
        public string Note { get; set; }
    }
}
