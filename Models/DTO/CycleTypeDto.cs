using CycleAPI.Models.Domain;

namespace CycleAPI.Models.DTO
{
    public class CycleTypeDto
    {
        public Guid TypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Cycle> Cycles { get; set; } = new List<Cycle>();
    }
}
