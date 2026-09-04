using System.ComponentModel.DataAnnotations;

namespace MayNghien.Infrastructures.Models.Requests
{
    public class SearchRequest
    {
        public List<Filter>? Filters { get; set; }

        public SortByInfo? SortBy { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int? PageIndex { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int? PageSize { get; set; }
    }
}
