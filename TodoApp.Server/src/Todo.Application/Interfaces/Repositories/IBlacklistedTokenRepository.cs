using Todo.Domain.Entities;

namespace Todo.Application.Interfaces.Repositories
{
    public interface IBlacklistedTokenRepository
    {
        Task AddAsync(BlacklistedToken entity);
        Task<bool> IsBlacklistedAsync(string token);
        Task ClearExpiredAsync();
    }
}
