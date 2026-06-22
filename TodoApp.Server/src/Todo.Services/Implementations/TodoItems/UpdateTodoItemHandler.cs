using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Domain.Entities;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoItems;
using Todo.Services.Mapping;

namespace Todo.Services.Implementations.TodoItems
{
    public class UpdateTodoItemHandler : IUpdateTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateTodoItemHandler(ITodoItemRepository todoItemRepository, 
            IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager)
        {
            _todoItemRepository = todoItemRepository;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        private async Task<ApplicationUser?> GetCurrentUser()
        => await _userManager.FindByEmailAsync(_contextAccessor.HttpContext?.User.Identity?.Name!);

        public async Task<AppResponse<TodoItemResponse>> HandleAsync(TodoItemRequest request)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                var user = await GetCurrentUser();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var task = await _todoItemRepository.GetAsync(request.Id);
                if (task == null || task.IsDeleted == true)
                    return result.BuildError("Item not found or deleted.");

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("SuperAdmin") && task.CreatedBy != user.Email)
                    return result.BuildError("Forbidden: you do not own this resource.");

                task.Title = request.Title;
                task.Description = request.Description;
                task.DueDate = request.DueDate;
                task.ModifiedBy = user.Email;
                task.ModifiedOn = DateTime.UtcNow;
                task.IsCompleted = request.IsCompleted;
                task.Priority = request.Priority;
                task.CompletedOn = request.IsCompleted ? request.CompletedOn ?? DateTime.UtcNow : null;
                await _todoItemRepository.EditAsync(task);

                var response = TodoItemMapper.ToResponse(task);
                return result.BuildResult(response, "Item updated successfully.");
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
