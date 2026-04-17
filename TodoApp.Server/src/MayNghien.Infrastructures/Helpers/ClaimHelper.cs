using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MayNghien.Infrastructures.Helpers
{
    public static class ClaimHelper
    {
        public static string GetTokenFromHeader(HttpContext httpContext)
        {
            // Check if the "Authorization" header exists
            if (httpContext.Request.Headers.TryGetValue("Authorization", out var headerValue))
            {
                // The header value should be in the format "Bearer <token>"
                var token = headerValue.ToString().Replace("Bearer ", "").Trim();
                return token;
            }

            // Header not found or does not contain a valid token
            return null;
        }
        public static string GetClaimByName(IHttpContextAccessor context, string clainName)
        {
            return context.HttpContext.User.Claims.First(x => x.Type == clainName).Value;
        }
    }
}
