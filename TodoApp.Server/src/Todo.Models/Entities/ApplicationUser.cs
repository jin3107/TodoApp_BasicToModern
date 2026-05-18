using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Commons.Enums;

namespace Todo.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Role? Role { get; set; }
        public string? LastLoginIp { get; set; }
    }
}
