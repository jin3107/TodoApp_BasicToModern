using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Auth.Requests;

namespace Todo.Application.Interfaces.Authentication
{
    public interface IVerifyOtpHandler
    {
        Task<AppResponse<bool>> HandleAsync(VerifyOtpRequest request);
    }
}
