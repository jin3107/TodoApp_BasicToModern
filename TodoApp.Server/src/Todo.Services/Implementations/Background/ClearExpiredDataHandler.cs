using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.Background;

namespace Todo.Services.Implementations.Background
{
    public class ClearExpiredDataHandler : IClearExpiredDataHandler
    {
        private readonly IBlacklistedTokenRepository _blacklistRepository;
        private readonly IOtpCodeRepository _otpRepository;
        private readonly ILogger<ClearExpiredDataHandler> _logger;

        public ClearExpiredDataHandler(IBlacklistedTokenRepository blacklistRepository, 
            IOtpCodeRepository otpRepository, ILogger<ClearExpiredDataHandler> logger)
        {
            _blacklistRepository = blacklistRepository;
            _otpRepository = otpRepository;
            _logger = logger;
        }

        public async Task HandleAsync()
        {
            _logger.LogInformation("Clearing expired tokens and OTPs at {Time}", DateTime.UtcNow);
            await _blacklistRepository.ClearExpiredAsync();
            await ClearExpiredOtpsAsync();
            _logger.LogInformation("Cleanup completed");
        }

        private async Task ClearExpiredOtpsAsync()
        {
            var expired = await _otpRepository.AsQueryable()
                .Where(o => o.ExpiresAt <= DateTime.UtcNow || o.IsUsed)
                .ToListAsync();
            foreach (var otp in expired)
                await _otpRepository.DeleteAsync(otp);
        }
    }
}
