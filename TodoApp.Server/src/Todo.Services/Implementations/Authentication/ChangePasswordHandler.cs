using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Domain.Entities;
using Todo.Domain.Enums;
using Todo.DTOs.Auth.Requests;
using Todo.DTOs.Auth.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.Authentication;

namespace Todo.Services.Implementations.Authentication
{
    public class ChangePasswordHandler : IChangePasswordHandler
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpCodeRepository _otpRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<ChangePasswordHandler> _logger;

        public ChangePasswordHandler(UserManager<ApplicationUser> userManager,
            IOtpCodeRepository otpRepository, IRefreshTokenRepository refreshTokenRepository,
            ILogger<ChangePasswordHandler> logger)
        {
            _userManager = userManager;
            _otpRepository = otpRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        public async Task<AppResponse<ChangePasswordResponse>> HandleAsync(ChangePasswordRequest request)
        {
            var result = new AppResponse<ChangePasswordResponse>();
            try
            {
                var email = request.Email.Trim().ToLowerInvariant();
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return result.BuildError("User not found.");

                var verifiedOtp = await _otpRepository.AsQueryable()
                    .Where(o => !o.IsDeleted
                             && o.Email == email
                             && o.Purpose == OtpPurpose.ChangePassword
                             && o.IsUsed
                             && o.ModifiedOn != null
                             && o.ModifiedOn >= DateTime.UtcNow.AddMinutes(-10))
                    .OrderByDescending(o => o.ModifiedOn)
                    .FirstOrDefaultAsync();

                if (verifiedOtp == null)
                    return result.BuildError("The OTP has not been verified, or the verification session has expired.");

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
                if (!resetResult.Succeeded)
                    return result.BuildError(string.Join(", ", resetResult.Errors.Select(e => e.Description)));

                await _refreshTokenRepository.RevokeByUserIdAsync(Guid.Parse(user.Id));

                verifiedOtp.IsDeleted = true;
                verifiedOtp.ModifiedOn = DateTime.UtcNow;
                await _otpRepository.EditAsync(verifiedOtp);

                return result.BuildResult(new ChangePasswordResponse
                {
                    Email = user.Email!
                }, "Password changed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangePassword failed");
                return result.BuildError(ex.Message);
            }
        }
    }
}