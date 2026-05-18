using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Todo.Commons.Helpers;
using Todo.DTOs.Auth.Requests;
using Todo.Services.Interfaces.Authentication;

namespace Todo.API.Controllers
{
    [Route("authentication")]
    [ApiController]
    public class AuthenticationsController : ControllerBase
    {
        private readonly ILoginHandler _login;
        private readonly IRegisterHandler _register;
        private readonly IRefreshTokenHandler _refreshToken;
        private readonly ILogoutHandler _logout;
        private readonly IChangePasswordHandler _changePassword;
        private readonly ISendOtpHandler _sendOtp;
        private readonly IVerifyOtpHandler _verifyOtp;
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public AuthenticationsController(ILoginHandler login, IRegisterHandler register, 
            IRefreshTokenHandler refreshToken, ILogoutHandler logout, 
            IChangePasswordHandler changePassword, ISendOtpHandler sendOtp, 
            IVerifyOtpHandler verifyOtp, IHostEnvironment environment, 
            IConfiguration configuration)
        {
            _login = login;
            _register = register;
            _refreshToken = refreshToken;
            _logout = logout;
            _changePassword = changePassword;
            _sendOtp = sendOtp;
            _verifyOtp = verifyOtp;
            _environment = environment;
            _configuration = configuration;
        }

        private void SetAccessTokenCookie(string token)
        {
            var opts = CookieHelper.GetSecureCookieOptions(_environment, Request.IsHttps);
            Response.Cookies.Append("AuthToken", token, opts);
        }

        private void SetRefreshTokenCookie(string token)
        {
            var minutes = int.Parse(_configuration["Jwt:RefreshTokenExpiresIn"] ?? "10080");
            var opts = CookieHelper.GetRefreshTokenCookieOptions(_environment, Request.IsHttps, minutes);
            Response.Cookies.Append("RefreshToken", token, opts);
        }

        private void DeleteAuthCookies()
        {
            var opts = CookieHelper.GetDeleteCookieOptions(_environment, Request.IsHttps);
            Response.Cookies.Delete("AuthToken", opts);
            Response.Cookies.Delete("RefreshToken", opts);
        }

        private string GetIpAddress()
            => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _login.HandleAsync(request, GetIpAddress());
            if (result.IsSuccess && result.Data != null)
            {
                SetAccessTokenCookie(result.Data.AccessToken);
                SetRefreshTokenCookie(result.Data.RefreshToken);
                result.Data.AccessToken = null!;
                result.Data.RefreshToken = null!;
            }
            return Ok(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _register.HandleAsync(request);
            return Ok(result);
        }

        [HttpPost("send-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
            => Ok(await _sendOtp.HandleAsync(request));

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
            => Ok(await _verifyOtp.HandleAsync(request));

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
            => Ok(await _changePassword.HandleAsync(request));

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized(new { message = "No refresh token provided." });

            var result = await _refreshToken.HandleAsync(refreshToken, GetIpAddress());
            if (result.IsSuccess && result.Data != null)
            {
                SetAccessTokenCookie(result.Data.Token);
                SetRefreshTokenCookie(result.Data.RefreshToken);
                result.Data.Token = null!;
                result.Data.RefreshToken = null!;
            }
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["RefreshToken"] ?? string.Empty;
            var accessToken = Request.Cookies["AuthToken"] ?? string.Empty;
            await _logout.HandleAsync(refreshToken, accessToken);
            DeleteAuthCookies();
            return Ok(new { message = "Logged out successfully." });
        }
    }
}
