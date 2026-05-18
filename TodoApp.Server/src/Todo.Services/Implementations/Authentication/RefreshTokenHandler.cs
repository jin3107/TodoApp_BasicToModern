using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Auth.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.Authentication;
using Todo.Services.Interfaces.Background;

namespace Todo.Services.Implementations.Authentication
{
    public class RefreshTokenHandler : IRefreshTokenHandler
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RefreshTokenHandler> _logger;

        public RefreshTokenHandler(UserManager<ApplicationUser> userManager, 
            IRefreshTokenRepository refreshTokenRepository, IEmailService emailService, 
            IConfiguration configuration, ILogger<RefreshTokenHandler> logger)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AppResponse<RefreshTokenResponse>> HandleAsync(string refreshToken, string ipAddress)
        {
            var result = new AppResponse<RefreshTokenResponse>();
            try
            {
                var tokenEntity = await _refreshTokenRepository.FindByTokenAsync(refreshToken);
                if (tokenEntity == null)
                    return result.BuildError("Invalid refresh token.");
                if (tokenEntity.IsRevoked)
                    return result.BuildError("Refresh token has been revoked.");
                if (tokenEntity.RefreshTokenExpiryTime < DateTime.UtcNow)
                    return result.BuildError("Refresh token has expired.");

                var user = await _userManager.FindByIdAsync(tokenEntity.UserId.ToString()!);
                if (user == null)
                    return result.BuildError("User not found.");

                if (!string.IsNullOrEmpty(user.LastLoginIp) && user.LastLoginIp != ipAddress)
                {
                    _logger.LogWarning("IP mismatch for {Email}: last={Last} current={Current}",
                        user.Email, user.LastLoginIp, ipAddress);
                    await _emailService.SendEmailAsync(
                        user.Email!,
                        "Warning: Login from new IP",
                        $"Refresh token detected from new IP: {ipAddress}\n" +
                        $"Last logged-in IP: {user.LastLoginIp}\n" +
                        $"If it's not you, change your password immediately.");
                }

                tokenEntity.IsRevoked = true;
                await _refreshTokenRepository.EditAsync(tokenEntity);

                user.LastLoginIp = ipAddress;
                await _userManager.UpdateAsync(user);

                var roles = await _userManager.GetRolesAsync(user);
                var claims = BuildClaims(user.Email!, roles);
                var (newAccessToken, newRefreshToken) = await GenerateTokensAsync(user, claims);

                return result.BuildResult(new RefreshTokenResponse
                {
                    Name = user.UserName!,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber!,
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshToken failed");
                return result.BuildError(ex.Message);
            }
        }

        private static List<Claim> BuildClaims(string email, IList<string> roles)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, email),
        };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));
            return claims;
        }

        private async Task<(string, string)> GenerateTokensAsync(ApplicationUser user, IEnumerable<Claim> claims)
        {
            var accessToken = GenerateAccessToken(claims);
            var refreshToken = GenerateRefreshToken();
            await _refreshTokenRepository.AddAsync(new RefreshTokenModel
            {
                UserId = Guid.Parse(user.Id),
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:RefreshTokenExpiresIn"] ?? "10080")),
                IsRevoked = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = user.Email
            });
            return (accessToken, refreshToken);
        }

        private string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddSeconds(
                    int.Parse(_configuration["Jwt:AccessTokenExpiresIn"] ?? "3600")),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
