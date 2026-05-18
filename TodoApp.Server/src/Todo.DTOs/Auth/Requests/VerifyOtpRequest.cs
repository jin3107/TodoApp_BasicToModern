using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Commons.Enums;

namespace Todo.DTOs.Auth.Requests
{
    public class VerifyOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; }

        public OtpPurpose Purpose { get; set; }
    }
}
