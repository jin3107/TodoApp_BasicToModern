using MayNghien.Infrastructures.Models.Responses;
using Todo.Domain.Entities;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.TodoItems;
using Todo.Application.Mapping;
using Microsoft.Extensions.Logging;

namespace Todo.Application.Implementations.TodoItems
{
    public class CreateTodoItemHandler : ICreateTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IUserService _userService;
        private readonly ILogger<CreateTodoItemHandler> _logger;

        public CreateTodoItemHandler(ITodoItemRepository todoItemRepository, IUserService userService, ILogger<CreateTodoItemHandler> logger)
        {
            _todoItemRepository = todoItemRepository;
            _userService = userService;
            _logger = logger;
        }

        public async Task<AppResponse<TodoItemResponse>> HandleAsync(TodoItemRequest request)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var newTask = new TodoItem
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    Description = request.Description,
                    DueDate = request.DueDate,
                    Priority = request.Priority,
                    TodoListId = request.TodoListId,
                    IsCompleted = false,
                    CompletedOn = null,
                };
                newTask.SetCreatedInfo(user.Id);
                await _todoItemRepository.AddAsync(newTask);

                var response = TodoItemMapper.ToResponse(newTask);
                return result.BuildResult(response, "Item created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Item: {Title} create failed.", request.Title);
                return result.BuildError("An error occurred while creating the item.");
            }
        }
    }
}
