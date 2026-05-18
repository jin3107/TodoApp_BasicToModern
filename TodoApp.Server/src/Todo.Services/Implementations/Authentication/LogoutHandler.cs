using MayNghien.Infrastructures.Models.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Auth.Requests;
using Todo.DTOs.Auth.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.Authentication;

namespace Todo.Services.Implementations.Authentication
{
    public class LogoutHandler : ILogoutHandler
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IBlacklistedTokenRepository _blacklistedTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LogoutHandler> _logger;

        public LogoutHandler(IRefreshTokenRepository refreshTokenRepository, 
            IBlacklistedTokenRepository blacklistedTokenRepository,
            IConfiguration configuration, ILogger<LogoutHandler> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _blacklistedTokenRepository = blacklistedTokenRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task HandleAsync(string refreshToken, string accessToken)
        {
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var tokenEntity = await _refreshTokenRepository.FindByTokenAsync(refreshToken);
                if (tokenEntity != null && !tokenEntity.IsRevoked)
                {
                    tokenEntity.IsRevoked = true;
                    await _refreshTokenRepository.EditAsync(tokenEntity);
                }
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var expiresIn = int.Parse(_configuration["Jwt:AccessTokenExpiresIn"] ?? "3600");
                await _blacklistedTokenRepository.AddAsync(new BlacklistedToken
                {
                    Token = accessToken,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn),
                    CreatedOn = DateTime.UtcNow
                });
            }
        }
    }
}
