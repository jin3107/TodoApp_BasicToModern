using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Todo.DTOs.Auth;
using Todo.Models.Entities;
using Todo.Application.Interfaces;

namespace Todo.Infrastructure.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserService(UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }

        public async Task<CurrentUserDto?> GetCurrentUserAsync()
        {
            var email = _contextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return null;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new CurrentUserDto { Id = user.Id, Email = user.Email!, Roles = roles };
        }
    }
}
