using Microsoft.EntityFrameworkCore;
using Todo.Application.Interfaces.Repositories;
using Todo.Domain.Entities;
using Todo.Models.Data;

namespace Todo.Infrastructure.Persistence.Repositories
{
    public class BlacklistedTokenRepository : IBlacklistedTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public BlacklistedTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(BlacklistedToken entity)
        {
            entity.CreatedOn ??= DateTime.UtcNow;
            entity.CreatedBy ??= "";
            await _context.BlacklistedTokens.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsBlacklistedAsync(string token)
            => await _context.BlacklistedTokens
                .AnyAsync(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow);

        public async Task ClearExpiredAsync()
        {
            var expired = await _context.BlacklistedTokens
                .Where(t => t.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
            _context.BlacklistedTokens.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
    }
}
