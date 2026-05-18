using MayNghien.Infrastructures.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Models.Data;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;

namespace Todo.Repositories.Implementations
{
    public class BlacklistedTokenRepository : GenericRepository<BlacklistedToken, ApplicationDbContext, ApplicationUser>, IBlacklistedTokenRepository
    {
        public BlacklistedTokenRepository(ApplicationDbContext unitOfWork) : base(unitOfWork)
        {
        }

        public async Task ClearExpiredAsync()
        {
            var expired = await _context.Set<BlacklistedToken>()
                .Where(t => t.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
            _context.Set<BlacklistedToken>().RemoveRange(expired);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsBlacklistedAsync(string token)
            => await _context.Set<BlacklistedToken>()
            .AnyAsync(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow);
    }
}
