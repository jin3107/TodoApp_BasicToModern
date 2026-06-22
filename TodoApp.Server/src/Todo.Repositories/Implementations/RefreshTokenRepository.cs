using MayNghien.Infrastructures.Repository;
using Microsoft.EntityFrameworkCore;
using Todo.Models.Data;
using Todo.Domain.Entities;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;

namespace Todo.Repositories.Implementations
{
    public class RefreshTokenRepository : GenericRepository<RefreshTokenModel, ApplicationDbContext, ApplicationUser>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<RefreshTokenModel?> FindByTokenAsync(string token)
        {
            return await _context.Set<RefreshTokenModel>()
                .FirstOrDefaultAsync(rt => rt.RefreshToken == token && !rt.IsRevoked);
        }

        public async Task RevokeByUserIdAsync(Guid userId)
        {
            var activeTokens = await _context.Set<RefreshTokenModel>()
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in activeTokens)
                token.IsRevoked = true;

            await _context.SaveChangesAsync();
        }
    }
}
