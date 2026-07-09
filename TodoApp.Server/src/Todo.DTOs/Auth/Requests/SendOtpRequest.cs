using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Domain.Enums;

namespace Todo.DTOs.Auth.Requests
{
    public class SendOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public OtpPurpose Purpose { get; set; }
    }
}
