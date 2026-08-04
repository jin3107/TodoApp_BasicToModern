using MayNghien.Infrastructures.Models.Responses;
using Microsoft.Extensions.Logging;
using Todo.DTOs.Responses;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.TodoItems;
using Todo.Application.Mapping;

namespace Todo.Application.Implementations.TodoItems
{
    public class GetTodoItemByIdHandler : IGetTodoItemByIdHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IUserService _userService;
        private readonly ILogger<GetTodoItemByIdHandler> _logger;

        public GetTodoItemByIdHandler(ITodoItemRepository todoItemRepository, IUserService userService, ILogger<GetTodoItemByIdHandler> logger)
        {
            _todoItemRepository = todoItemRepository;
            _userService = userService;
            _logger = logger;
        }

        public async Task<AppResponse<TodoItemResponse>> HandleAsync(Guid id)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return result.BuildError("Unauthorized");

                var task = await _todoItemRepository.GetByIdAsync(id);
                if (task == null || task.IsDeleted == true)
                    return result.BuildError("Item not found or deleted.");

                var response = TodoItemMapper.ToResponse(task);
                return result.BuildResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get TodoItem {ItemId}", id);
                return result.BuildError("An error occurred while retrieving the item.");
            }
        }
    }
}
