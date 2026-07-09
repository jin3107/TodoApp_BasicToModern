using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Auth.Responses;

namespace Todo.Application.Interfaces.Authentication
{
    public interface IRefreshTokenHandler
    {
        Task<AppResponse<RefreshTokenResponse>> HandleAsync(string refreshToken, string ipAddress);
    }
}
