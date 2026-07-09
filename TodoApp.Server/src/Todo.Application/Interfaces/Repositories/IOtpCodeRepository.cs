using Todo.Domain.Entities;
using Todo.Domain.Enums;

namespace Todo.Application.Interfaces.Repositories
{
    public interface IOtpCodeRepository
    {
        Task AddAsync(OtpCode entity);
        Task UpdateAsync(OtpCode entity);
        Task<OtpCode?> FindActiveAsync(string email, OtpPurpose purpose);
        Task<OtpCode?> FindRecentlyVerifiedAsync(string email, OtpPurpose purpose);
        Task InvalidatePreviousAsync(string email, OtpPurpose purpose);
        Task ClearExpiredAsync();
    }
}
