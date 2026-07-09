using Microsoft.EntityFrameworkCore;
using Todo.Application.Interfaces.Repositories;
using Todo.Domain.Entities;
using Todo.Models.Data;

namespace Todo.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshTokenModel entity)
        {
            entity.CreatedOn ??= DateTime.UtcNow;
            entity.CreatedBy ??= "";
            await _context.RefreshTokens.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RefreshTokenModel entity)
        {
            _context.RefreshTokens.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshTokenModel?> FindByTokenAsync(string token)
            => await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.RefreshToken == token && !rt.IsRevoked);

        public async Task RevokeByUserIdAsync(Guid userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in activeTokens)
                token.IsRevoked = true;

            await _context.SaveChangesAsync();
        }
    }
}
