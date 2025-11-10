using MayNghien.Infrastructures.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Commons.Enums;

namespace Todo.DTOs.Requests
{
    public class TodoItemRequest
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title must not exceed 200 characters")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public Tier Priority { get; set; }
        public DateTime? CompletedOn { get; set; }
    }
}
