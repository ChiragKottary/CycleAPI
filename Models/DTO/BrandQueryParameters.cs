using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Models.DTO
{
    public class BrandQueryParameters : BaseQueryParameters
    {
        public bool? IsActive { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? SearchTerm { get; set; }
        public int? MinCycles { get; set; }
        public int? MaxCycles { get; set; }
    }
}