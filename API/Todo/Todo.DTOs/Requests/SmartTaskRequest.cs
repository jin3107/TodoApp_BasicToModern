using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.DTOs.Requests
{
    public class BulkUpdateRequest
    {
        [Required]
        public List<Guid> TaskIds { get; set; }

        [Required]
        public string Operation { get; set; }

        public string Value { get; set; }  
    }

    public class SmartSuggesstionRequest
    {
        public Guid? UserId { get; set; }
        public int MaxSuggesstions { get; set; } = 5;

        // all, overdue, priority, schedule
        public string SuggesstionType { get; set; } = "all";
    }
}
