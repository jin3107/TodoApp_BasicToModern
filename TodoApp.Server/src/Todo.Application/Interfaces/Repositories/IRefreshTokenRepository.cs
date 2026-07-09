using Todo.Domain.Entities;

namespace Todo.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshTokenModel entity);
        Task UpdateAsync(RefreshTokenModel entity);
        Task<RefreshTokenModel?> FindByTokenAsync(string token);
        Task RevokeByUserIdAsync(Guid userId);
    }
}
