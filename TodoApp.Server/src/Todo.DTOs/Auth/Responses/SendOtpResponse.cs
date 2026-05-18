using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.DTOs.Auth.Responses
{
    public class SendOtpResponse
    {
        public string Email { get; set; }
        public string Message { get; set; }
    }
}
