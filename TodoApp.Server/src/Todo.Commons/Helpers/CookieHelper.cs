using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Todo.Commons.Helpers
{
    public static class CookieHelper
    {
        public static CookieOptions GetSecureCookieOptions(IHostEnvironment environment)
        {
            var isDev = environment.IsDevelopment();
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = isDev ? SameSiteMode.None : SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(24),
                Path = "/",
                IsEssential = true
            };
        }

        public static CookieOptions GetRefreshTokenCookieOptions(IHostEnvironment environment, int expiresMinutes = 10080)
        {
            var isDev = environment.IsDevelopment();
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = isDev ? SameSiteMode.None : SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(expiresMinutes),
                Path = "/",
                IsEssential = true
            };
        }

        public static CookieOptions GetDeleteCookieOptions(IHostEnvironment environment)
        {
            var isDev = environment.IsDevelopment();
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = isDev ? SameSiteMode.None : SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            };
        }
    }
}
