using System.Text.Json.Serialization;

namespace CycleAPI.Models.DTO.Common
{
    public class BaseQueryParameters
    {
        // private const int MaxPageSize = 100;
        // private int _pageSize = 50;
        // private const int MaxPageSize = 50;
        // private int _pageSize = 15;
        private const int MaxPageSize = 35;
        private int _pageSize = 7;

        public int Page { get; set; } = 1;
        
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? SortBy { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

        public string? SearchTerm { get; set; }
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }
}