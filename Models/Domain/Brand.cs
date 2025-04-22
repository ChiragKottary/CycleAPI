namespace CycleAPI.Models.Domain
{
    public class Brand
    {
        public Guid BrandId { get; set; }
        public string BrandName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Cycle> Cycles { get; set; }
    }
}
