namespace CycleAPI.Models.DTO
{
    public class BrandDto
    {
        public Guid BrandId { get; set; }

        public string BrandName { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
