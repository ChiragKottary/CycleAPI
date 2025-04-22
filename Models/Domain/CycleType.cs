namespace CycleAPI.Models.Domain
{
    public class CycleType
    {
        public Guid TypeId { get; set; }

        public string TypeName { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

   
}
