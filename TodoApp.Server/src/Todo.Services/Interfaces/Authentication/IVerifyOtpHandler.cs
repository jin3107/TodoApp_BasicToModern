using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Auth.Requests;

namespace Todo.Services.Interfaces.Authentication
{
    public interface IVerifyOtpHandler
    {
        Task<AppResponse<bool>> HandleAsync(VerifyOtpRequest request);
    }
}
