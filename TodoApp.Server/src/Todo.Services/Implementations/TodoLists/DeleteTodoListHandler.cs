using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoLists;

namespace Todo.Services.Implementations.TodoLists
{
    public class DeleteTodoListHandler : IDeleteTodoListHandler
    {
        private readonly ITodoListRepository _todoListRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteTodoListHandler(ITodoListRepository todoListRepository,
            IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager)
        {
            _todoListRepository = todoListRepository;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        private async Task<ApplicationUser?> GetCurrentUser()
        => await _userManager.FindByEmailAsync(_contextAccessor.HttpContext?.User.Identity?.Name!);

        public async Task<AppResponse<string>> HandleAsync(Guid id)
        {
            var result = new AppResponse<string>();
            try
            {
                var user = await GetCurrentUser();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var entity = await _todoListRepository.GetAsync(id);
                if (entity == null || entity.IsDeleted == true)
                    return result.BuildError("Todo list not found or deleted.");

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("SuperAdmin") && entity.CreatedBy != user.Email)
                    return result.BuildError("Forbidden: you do not own this resource.");
                entity.IsDeleted = true;
                await _todoListRepository.EditAsync(entity);
                return result.BuildResult("Todo list deleted successfully.");
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
