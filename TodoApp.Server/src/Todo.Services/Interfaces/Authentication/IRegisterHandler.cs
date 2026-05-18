using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Auth.Requests;
using Todo.DTOs.Auth.Responses;

namespace Todo.Services.Interfaces.Authentication
{
    public interface IRegisterHandler
    {
        Task<AppResponse<RegisterResponse>> HandleAsync(RegisterRequest request);
    }
}
