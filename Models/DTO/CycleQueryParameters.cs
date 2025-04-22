using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Models.DTO
{
    public class CycleQueryParameters : BaseQueryParameters
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public Guid? BrandId { get; set; }
        public Guid? TypeId { get; set; }
        public bool? IsActive { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
    }
}