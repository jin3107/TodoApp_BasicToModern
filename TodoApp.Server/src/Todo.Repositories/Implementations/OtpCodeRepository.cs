using MayNghien.Infrastructures.Repository;
using Microsoft.EntityFrameworkCore;
using Todo.Domain.Entities;
using Todo.Domain.Enums;
using Todo.Models.Data;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;

namespace Todo.Repositories.Implementations
{
    public class OtpCodeRepository : GenericRepository<OtpCode, ApplicationDbContext, ApplicationUser>, IOtpCodeRepository
    {
        public OtpCodeRepository(ApplicationDbContext unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<OtpCode?> FindActiveAsync(string email, OtpPurpose purpose)
        {
            var normalizedEmail = email.Trim().ToLower();
            return await _context.Set<OtpCode>()
                .Where(o => !o.IsDeleted
                    && o.Email == normalizedEmail
                    && o.Purpose == purpose
                    && !o.IsUsed
                    && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefaultAsync();
        }

        public async Task InvalidatePreviousAsync(string email, OtpPurpose purpose)
        {
            var normalizedEmail = email.Trim().ToLower();
            var previous = await _context.Set<OtpCode>()
                .Where(o => !o.IsDeleted && o.Email == normalizedEmail && o.Purpose == purpose
                    && !o.IsUsed).ToListAsync();
            foreach (var otp in previous)
                otp.IsUsed = true;

            await _context.SaveChangesAsync();
        }
    }
}