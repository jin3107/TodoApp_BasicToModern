using MayNghien.Infrastructures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.Models.Entities
{
    public class BlacklistedToken : BaseEntity
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
