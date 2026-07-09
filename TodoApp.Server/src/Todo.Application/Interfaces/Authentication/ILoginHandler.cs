using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Auth.Requests;
using Todo.DTOs.Auth.Responses;

namespace Todo.Application.Interfaces.Authentication
{
    public interface ILoginHandler
    {
        Task<AppResponse<LoginResponse>> HandleAsync(LoginRequest request, string ipAddress);
    }
}
