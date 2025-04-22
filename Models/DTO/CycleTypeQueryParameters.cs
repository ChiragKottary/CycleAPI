using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Models.DTO
{
    public class CycleTypeQueryParameters : BaseQueryParameters
    {
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}