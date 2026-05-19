using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Todo.Commons.Enums;
using Todo.DTOs.Auth.Requests;
using Todo.DTOs.Auth.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.Authentication;
using Todo.Services.Interfaces.Background;

namespace Todo.Services.Implementations.Authentication
{
    public class SendOtpHandler : ISendOtpHandler
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpCodeRepository _otpRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<SendOtpHandler> _logger;

        public SendOtpHandler(UserManager<ApplicationUser> userManager, 
            IOtpCodeRepository otpRepository, IEmailService emailService, 
            ILogger<SendOtpHandler> logger)
        {
            _userManager = userManager;
            _otpRepository = otpRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AppResponse<SendOtpResponse>> HandleAsync(SendOtpRequest request)
        {
            var result = new AppResponse<SendOtpResponse>();
            try
            {
                var email = request.Email.Trim().ToLowerInvariant();
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return result.BuildError("Email address does not exist.");

                await _otpRepository.InvalidatePreviousAsync(email, request.Purpose);

                var otp = GenerateOtp();
                await _otpRepository.AddAsync(new OtpCode
                {
                    Email = email,
                    Code = otp,
                    Purpose = request.Purpose,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false,
                    AttemptCount = 0,
                    CreatedOn = DateTime.UtcNow
                });

                var subject = request.Purpose == OtpPurpose.ChangePassword
                    ? "Confirm your TodoApp password change."
                    : "Verify your TodoApp account";

                await _emailService.SendEmailAsync(email, subject,
                    $"Your OTP code is: {otp}\nThe code is valid for 5 minutes.");

                return result.BuildResult(new SendOtpResponse
                {
                    Email = email,
                    Message = "The OTP has been sent to your email."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendOtp failed");
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
