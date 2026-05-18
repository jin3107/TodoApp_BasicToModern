using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Auth.Responses;

namespace Todo.Services.Interfaces.Authentication
{
    public interface IRefreshTokenHandler
    {
        Task<AppResponse<RefreshTokenResponse>> HandleAsync(string refreshToken, string ipAddress);
    }
}
