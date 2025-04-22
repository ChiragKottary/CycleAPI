using CycleAPI.Models.Domain;

namespace CycleAPI.Models.DTO
{
    public class CreateCycleRequestDto
    {
        public string ModelName { get; set; }
        public Guid BrandId { get; set; }
        public Guid TypeId { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public string ImageUrl { get; set; }

    }
}
