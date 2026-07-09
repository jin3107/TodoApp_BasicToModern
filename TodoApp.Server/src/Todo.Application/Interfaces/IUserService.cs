using Todo.DTOs.Auth;

namespace Todo.Application.Interfaces
{
    public interface IUserService
    {
        Task<CurrentUserDto?> GetCurrentUserAsync();
    }
}
