using Microsoft.Extensions.Logging;
using Todo.Application.Interfaces.Background;
using Todo.Application.Interfaces.Repositories;

namespace Todo.Application.Implementations.Background
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
            await _otpRepository.ClearExpiredAsync();
            _logger.LogInformation("Cleanup completed");
        }
    }
}
