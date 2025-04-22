namespace CycleAPI.Models.DTO
{
    public class CycleTypeDto
    {
        public Guid TypeId { get; set; }

        public string TypeName { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
