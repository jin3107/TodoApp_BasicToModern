using MayNghien.Infrastructures.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Commons.Enums;
using Todo.Models.Data;
using Todo.Models.Entities;

namespace Todo.Repositories.Interfaces
{
    public interface IOtpCodeRepository : IGenericRepository<OtpCode, ApplicationDbContext, ApplicationUser>
    {
        Task<OtpCode?> FindActiveAsync(string email, OtpPurpose purpose);
        Task InvalidatePreviousAsync(string email, OtpPurpose purpose);
    }
}
