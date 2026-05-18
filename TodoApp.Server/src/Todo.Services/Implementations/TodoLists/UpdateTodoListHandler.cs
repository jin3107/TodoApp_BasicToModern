using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoLists;
using Todo.Services.Mapping;

namespace Todo.Services.Implementations.TodoLists
{
    public class UpdateTodoListHandler : IUpdateTodoListHandler
    {
        private readonly ITodoListRepository _todoListRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateTodoListHandler(ITodoListRepository todoListRepository, 
            IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager)
        {
            _todoListRepository = todoListRepository;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        private async Task<ApplicationUser?> GetCurrentUser()
        => await _userManager.FindByEmailAsync(_contextAccessor.HttpContext?.User.Identity?.Name!);

        public async Task<AppResponse<TodoListResponse>> HandleAsync(TodoListRequest request)
        {
            var result = new AppResponse<TodoListResponse>();
            try
            {
                var user = await GetCurrentUser();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var entity = await _todoListRepository.GetAsync(request.Id);
                if (entity == null || entity.IsDeleted == true)
                    return result.BuildError("Todo list not found or deleted.");

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("SuperAdmin") && entity.CreatedBy != user.Email)
                    return result.BuildError("Forbidden: you do not own this resource.");

                entity.Name = request.Name;
                entity.Description = request.Description;
                entity.ModifiedBy = user.Email;
                entity.ModifiedOn = DateTime.UtcNow;
                await _todoListRepository.EditAsync(entity);

                var response = TodoListMapper.ToResponse(entity);
                return result.BuildResult(response, "Todo list updated successfully.");
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
