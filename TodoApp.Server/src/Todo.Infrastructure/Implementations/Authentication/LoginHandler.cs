using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Todo.Domain.Entities;
using Todo.Domain.Enums;
using Role = Todo.Commons.Enums.Role;
using Todo.DTOs.Auth.Requests;
using Todo.DTOs.Auth.Responses;
using Todo.Models.Entities;
using Todo.Application.Interfaces.Authentication;
using Todo.Application.Interfaces.Repositories;

namespace Todo.Infrastructure.Implementations.Authentication
{
    public class LoginHandler : ILoginHandler
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<LoginHandler> _logger;

        public LoginHandler(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IConfiguration configuration,
            IRefreshTokenRepository refreshTokenRepository, ILogger<LoginHandler> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        public async Task<AppResponse<LoginResponse>> HandleAsync(LoginRequest request, string ipAddress)
        {
            var result = new AppResponse<LoginResponse>();
            try
            {
                var user = await _userManager.FindByNameAsync(request.UserName)
                           ?? await _userManager.FindByEmailAsync(request.UserName);
                var isBootstrapAdmin = IsBootstrapAdmin(request.UserName);

                if (user == null && isBootstrapAdmin)
                    user = await CreateAdminAsync(request.UserName);

                const string invalidCredentialsMessage = "Invalid username or password.";

                if (user == null)
                    return result.BuildError(invalidCredentialsMessage);

                if (await _userManager.IsLockedOutAsync(user))
                    return result.BuildError("Account is temporarily locked due to too many failed login attempts. Please try again later.");

                if (!await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    await _userManager.AccessFailedAsync(user);
                    return result.BuildError(invalidCredentialsMessage);
                }

                if (await _userManager.GetAccessFailedCountAsync(user) > 0)
                    await _userManager.ResetAccessFailedCountAsync(user);

                if (!user.EmailConfirmed)
                    return result.BuildError("Email not verified. Please verify your email first.");

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Any())
                    return result.BuildError("User has no role assigned.");

                user.LastLoginIp = ipAddress;
                await _userManager.UpdateAsync(user);

                var claims = BuildClaims(user.Email!, roles);
                var (accessToken, refreshToken) = await GenerateTokensAsync(user, claims);

                return result.BuildResult(new LoginResponse
                {
                    UserName = user.UserName!,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber!,
                    Role = roles.First(),
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return result.BuildError(ex.Message);
            }
        }

        private async Task<ApplicationUser> CreateAdminAsync(string email)
        {
            var admin = new ApplicationUser
            {
                Email = email,
                EmailConfirmed = true,
                UserName = email,
                Role = Role.SuperAdmin,
                LastLoginIp = string.Empty
            };
            var bootstrapAdminPassword = _configuration["Bootstrap:AdminPassword"]
                ?? throw new InvalidOperationException("Bootstrap:AdminPassword is not configured.");
            var createUserResult = await _userManager.CreateAsync(admin, bootstrapAdminPassword);
            if (!createUserResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", createUserResult.Errors.Select(e => e.Description)));

            var roleResult = await EnsureRoleAssignedAsync(admin, Role.SuperAdmin.ToString(), Role.SuperAdmin);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));

            return admin;
        }

        private bool IsBootstrapAdmin(string userName)
        {
            var bootstrapAdminEmail = _configuration["Bootstrap:AdminEmail"];
            return !string.IsNullOrEmpty(bootstrapAdminEmail)
                && string.Equals(userName, bootstrapAdminEmail, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<IdentityResult> EnsureRoleAssignedAsync(ApplicationUser user, string roleName, Role appRole)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!createRoleResult.Succeeded)
                    return createRoleResult;
            }

            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!addRoleResult.Succeeded)
                    return addRoleResult;
            }

            if (user.Role != appRole)
            {
                user.Role = appRole;
                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                    return updateUserResult;
            }

            return IdentityResult.Success;
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

        private async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(
            ApplicationUser user, IEnumerable<Claim> claims)
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
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
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
