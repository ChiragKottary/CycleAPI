using CycleAPI.Models.DTO.Common;

namespace CycleAPI.Models.DTO
{
    public class CartQueryParameters : BaseQueryParameters
    {
        public bool? IsActive { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? CustomerName { get; set; }
    }
}