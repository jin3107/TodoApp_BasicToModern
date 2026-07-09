using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.TodoItems;
using Todo.Application.Mapping;

namespace Todo.Application.Implementations.TodoItems
{
    public class UpdateTodoItemHandler : IUpdateTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IUserService _userService;

        public UpdateTodoItemHandler(ITodoItemRepository todoItemRepository, IUserService userService)
        {
            _todoItemRepository = todoItemRepository;
            _userService = userService;
        }

        public async Task<AppResponse<TodoItemResponse>> HandleAsync(TodoItemRequest request)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var task = await _todoItemRepository.GetByIdAsync(request.Id);
                if (task == null || task.IsDeleted == true)
                    return result.BuildError("Item not found or deleted.");

                if (!user.Roles.Contains("SuperAdmin") && task.CreatedBy != user.Email)
                    return result.BuildError("Forbidden: you do not own this resource.");

                task.Title = request.Title;
                task.Description = request.Description;
                task.DueDate = request.DueDate;
                task.IsCompleted = request.IsCompleted;
                task.Priority = request.Priority;
                task.CompletedOn = request.IsCompleted ? request.CompletedOn ?? DateTime.UtcNow : null;
                task.SetModifiedInfo(user.Email);
                await _todoItemRepository.UpdateAsync(task);

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
