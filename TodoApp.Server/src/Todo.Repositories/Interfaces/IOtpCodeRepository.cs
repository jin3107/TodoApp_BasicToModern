using MayNghien.Infrastructures.Repository;
using Todo.Domain.Entities;
using Todo.Domain.Enums;
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
