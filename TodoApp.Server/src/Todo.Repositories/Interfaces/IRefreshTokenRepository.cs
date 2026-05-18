using MayNghien.Infrastructures.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Models.Data;
using Todo.Models.Entities;

namespace Todo.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshTokenModel, ApplicationDbContext, ApplicationUser>
    {
        Task<RefreshTokenModel?> FindByTokenAsync(string token);
        Task RevokeByUserIdAsync(Guid userId);
    }
}
