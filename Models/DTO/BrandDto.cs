using CycleAPI.Models.Domain;

namespace CycleAPI.Models.DTO
{
    public class BrandDto
    {
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public ICollection<Cycle> Cycles { get; set; } = new List<Cycle>();
    }
}
