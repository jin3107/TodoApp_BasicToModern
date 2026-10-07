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
            if (httpContext.Request.Headers.TryGetValue("Authorization", out var headerValue))
            {
                var token = headerValue.ToString().Replace("Bearer ", "").Trim();
                return token;
            }

            return null;
        }
        public static string GetClaimByName(IHttpContextAccessor context, string clainName)
        {
            return context.HttpContext.User.Claims.First(x => x.Type == clainName).Value;
        }
    }
}
