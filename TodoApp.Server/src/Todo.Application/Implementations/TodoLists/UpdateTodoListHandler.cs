using MayNghien.Infrastructures.Models.Responses;
using Microsoft.Extensions.Logging;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.TodoLists;
using Todo.Application.Mapping;

namespace Todo.Application.Implementations.TodoLists
{
    public class UpdateTodoListHandler : IUpdateTodoListHandler
    {
        private readonly ITodoListRepository _todoListRepository;
        private readonly IUserService _userService;
        private readonly ILogger<UpdateTodoListHandler> _logger;

        public UpdateTodoListHandler(ITodoListRepository todoListRepository, IUserService userService, ILogger<UpdateTodoListHandler> logger)
        {
            _todoListRepository = todoListRepository;
            _userService = userService;
            _logger = logger;
        }

        public async Task<AppResponse<TodoListResponse>> HandleAsync(TodoListRequest request)
        {
            var result = new AppResponse<TodoListResponse>();
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var entity = await _todoListRepository.GetByIdAsync(request.Id);
                if (entity == null || entity.IsDeleted == true)
                    return result.BuildError("Todo list not found or deleted.");

                if (!user.Roles.Contains("SuperAdmin") && entity.CreatedBy != user.Id)
                    return result.BuildError("Forbidden: you do not own this resource.");

                entity.Name = request.Name;
                entity.Description = request.Description;
                entity.SetModifiedInfo(user.Email);
                await _todoListRepository.UpdateAsync(entity);

                var response = TodoListMapper.ToResponse(entity);
                return result.BuildResult(response, "Todo list updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TodoList: {Id} update failed.", request.Id);
                return result.BuildError("An error occurred while updating the todo list.");
            }
        }
    }
}
