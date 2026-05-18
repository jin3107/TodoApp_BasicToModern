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
using Todo.Services.Interfaces.TodoItems;
using Todo.Services.Mapping;

namespace Todo.Services.Implementations.TodoItems
{
    public class CreateTodoItemHandler : ICreateTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateTodoItemHandler(ITodoItemRepository todoItemRepository, 
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

                var newTask = TodoItemMapper.ToEntity(request);
                newTask.Id = Guid.NewGuid();
                newTask.Title = request.Title;
                newTask.Description = request.Description;
                newTask.DueDate = request.DueDate;
                newTask.Priority = request.Priority;
                newTask.TodoListId = request.TodoListId;
                newTask.CreatedBy = user.Email;
                newTask.CreatedOn = DateTime.UtcNow;
                newTask.IsCompleted = false;
                newTask.CompletedOn = null;
                await _todoItemRepository.AddAsync(newTask);

                var response = TodoItemMapper.ToResponse(newTask);
                return result.BuildResult(response, "Item created successfully.");
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
