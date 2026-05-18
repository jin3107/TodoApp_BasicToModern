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
using Todo.Services.Interfaces.TodoItems;

namespace Todo.Services.Implementations.TodoItems
{
    public class DeleteTodoItemHandler : IDeleteTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteTodoItemHandler(ITodoItemRepository todoItemRepository, 
            IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager)
        {
            _todoItemRepository = todoItemRepository;
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

                var task = await _todoItemRepository.GetAsync(id);
                if (task == null || task.IsDeleted == true)
                    return result.BuildError("Item not found or deleted.");

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("SuperAdmin") && task.CreatedBy != user.Email)
                    return result.BuildError("Forbidden: you do not own this resource.");
                task.IsDeleted = true;
                await _todoItemRepository.EditAsync(task);
                return result.BuildResult("Item deleted successfully.");
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
