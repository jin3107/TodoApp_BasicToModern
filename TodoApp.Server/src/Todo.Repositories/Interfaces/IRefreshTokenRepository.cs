using MayNghien.Infrastructures.Repository;
using Todo.Models.Data;
using Todo.Domain.Entities;
using Todo.Models.Entities;

namespace Todo.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshTokenModel, ApplicationDbContext, ApplicationUser>
    {
        Task<RefreshTokenModel?> FindByTokenAsync(string token);
        Task RevokeByUserIdAsync(Guid userId);
    }
}
