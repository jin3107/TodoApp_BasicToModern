using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Todo.Domain.Entities;
using Todo.Domain.Enums;
using Role = Todo.Commons.Enums.Role;
using Todo.DTOs.Auth.Requests;
using Todo.DTOs.Auth.Responses;
using Todo.Models.Entities;
using Todo.Application.Interfaces.Authentication;
using Todo.Application.Interfaces.Background;
using Todo.Application.Interfaces.Repositories;

namespace Todo.Infrastructure.Implementations.Authentication
{
    public class RegisterHandler : IRegisterHandler
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOtpCodeRepository _otpCodeRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegisterHandler> _logger;

        public RegisterHandler(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IOtpCodeRepository otpCodeRepository,
            IEmailService emailService, ILogger<RegisterHandler> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _otpCodeRepository = otpCodeRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AppResponse<RegisterResponse>> HandleAsync(RegisterRequest request)
        {
            var result = new AppResponse<RegisterResponse>();
            try
            {
                var email = request.Email.Trim().ToLowerInvariant();

                if (await _userManager.FindByEmailAsync(email) != null)
                    return result.BuildError("Email already exists.");

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = false,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    PhoneNumber = request.PhoneNumber,
                    Role = Role.User
                };

                var createResult = await _userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                    return result.BuildError(string.Join(", ", createResult.Errors.Select(e => e.Description)));

                if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                await _userManager.AddToRoleAsync(user, "User");

                var otp = GenerateOtp();
                await _otpCodeRepository.InvalidatePreviousAsync(email, OtpPurpose.VerifyEmail);
                await _otpCodeRepository.AddAsync(new OtpCode
                {
                    Email = email,
                    Code = otp,
                    Purpose = OtpPurpose.VerifyEmail,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false,
                    AttemptCount = 0,
                    CreatedOn = DateTime.UtcNow
                });

                await _emailService.SendEmailAsync(
                    email,
                    "Confirm TodoApp account",
                    $"Your OTP code is: {otp}\nThe code is valid for 5 minutes.");

                return result.BuildResult(new RegisterResponse
                {
                    Email = email,
                    PhoneNumber = request.PhoneNumber,
                    Name = request.Name,
                    Role = "User",
                    AccessToken = string.Empty,
                    RefreshToken = string.Empty,
                }, "Registration successful. Please check your email to confirm your account.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed");
                return result.BuildError(ex.Message);
            }
        }

        private static string GenerateOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            return (Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000).ToString("D6");
        }
    }
}
