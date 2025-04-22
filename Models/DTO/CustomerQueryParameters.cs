using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Models.DTO
{
    public class CustomerQueryParameters : BaseQueryParameters
    {
        public string? City { get; set; }
        public string? State { get; set; }
        public DateTime? RegisteredFrom { get; set; }
        public DateTime? RegisteredTo { get; set; }
        public string? Email { get; set; }
        public bool? HasActiveCart { get; set; }
        public int? MinOrders { get; set; }
        public int? MaxOrders { get; set; }
        public string? Phone { get; set; }
        public bool? IsActive { get; set; }
    }
}