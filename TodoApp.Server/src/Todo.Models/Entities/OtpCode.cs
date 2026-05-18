using MayNghien.Infrastructures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Commons.Enums;

namespace Todo.Models.Entities
{
    public class OtpCode : BaseEntity
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public OtpPurpose Purpose { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public int AttemptCount { get; set; }
    }
}
