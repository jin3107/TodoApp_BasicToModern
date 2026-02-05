using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MayNghien.Infrastructures.Models.Requests
{
    public class SortByInfo
    {
        [Required]
        public string? FieldName { get; set; }

        public bool? Ascending { get; set; }=true;

        public SortByInfo() { }

        public SortByInfo(string fieldName, bool ascending)
        {
            FieldName = fieldName;
            Ascending = ascending;
        }
    }
}
