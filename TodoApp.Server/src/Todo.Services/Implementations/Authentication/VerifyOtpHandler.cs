using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Commons.Enums;
using Todo.DTOs.Auth.Requests;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.Authentication;

namespace Todo.Services.Implementations.Authentication
{
    public class VerifyOtpHandler : IVerifyOtpHandler
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOtpCodeRepository _otpRepository;
        private readonly ILogger<VerifyOtpHandler> _logger;

        public VerifyOtpHandler(UserManager<ApplicationUser> userManager, 
            IOtpCodeRepository otpRepository, ILogger<VerifyOtpHandler> logger)
        {
            _userManager = userManager;
            _otpRepository = otpRepository;
            _logger = logger;
        }

        public async Task<AppResponse<bool>> HandleAsync(VerifyOtpRequest request)
        {
            var result = new AppResponse<bool>();
            try
            {
                var otp = await _otpRepository.FindActiveAsync(request.Email, request.Purpose);
                if (otp == null)
                    return result.BuildError("The OTP is invalid or has expired.");

                if (otp.AttemptCount >= 5)
                {
                    otp.IsUsed = true;
                    await _otpRepository.EditAsync(otp);
                    return result.BuildError("The number of attempts has been exceeded. Please request a new OTP.");
                }

                if (otp.Code != request.Code)
                {
                    otp.AttemptCount++;
                    await _otpRepository.EditAsync(otp);
                    return result.BuildError($"OTP is incorrect. {5 - otp.AttemptCount} attempts remaining.");
                }

                otp.IsUsed = true;
                await _otpRepository.EditAsync(otp);

                if (request.Purpose == OtpPurpose.VerifyEmail)
                {
                    var user = await _userManager.FindByEmailAsync(request.Email);
                    if (user != null)
                    {
                        user.EmailConfirmed = true;
                        await _userManager.UpdateAsync(user);
                    }
                }

                return result.BuildResult(true, "Verification successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VerifyOtp failed");
                return result.BuildError(ex.Message);
            }
        }
    }
}
