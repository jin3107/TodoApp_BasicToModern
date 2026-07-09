using MayNghien.Infrastructures.Models.Responses;
using Todo.Domain.Entities;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.TodoItems;
using Todo.Application.Mapping;

namespace Todo.Application.Implementations.TodoItems
{
    public class CreateTodoItemHandler : ICreateTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IUserService _userService;

        public CreateTodoItemHandler(ITodoItemRepository todoItemRepository, IUserService userService)
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
                newTask.SetCreatedInfo(user.Email);
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
