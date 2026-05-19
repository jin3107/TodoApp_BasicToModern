using MayNghien.Infrastructures.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Commons.Enums;
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
<<<<<<< HEAD
            => await _context.Set<OtpCode>()
                .Where(o => o.Email == email
=======
        {
            var normalizedEmail = email.Trim().ToLower();
            return await _context.Set<OtpCode>()
                .Where(o => !o.IsDeleted
                    && o.Email == normalizedEmail
>>>>>>> dev
                    && o.Purpose == purpose
                    && !o.IsUsed
                    && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedOn)
                .FirstOrDefaultAsync();
<<<<<<< HEAD

        public async Task InvalidatePreviousAsync(string email, OtpPurpose purpose)
        {
            var previous = await _context.Set<OtpCode>()
                .Where(o => o.Email == email && o.Purpose == purpose
=======
        }

        public async Task InvalidatePreviousAsync(string email, OtpPurpose purpose)
        {
            var normalizedEmail = email.Trim().ToLower();
            var previous = await _context.Set<OtpCode>()
                .Where(o => !o.IsDeleted && o.Email == normalizedEmail && o.Purpose == purpose
>>>>>>> dev
                    && !o.IsUsed).ToListAsync();
            foreach (var otp in previous)
                otp.IsUsed = true;

            await _context.SaveChangesAsync();
        }
    }
}
