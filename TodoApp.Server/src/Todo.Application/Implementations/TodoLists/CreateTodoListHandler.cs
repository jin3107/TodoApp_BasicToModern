using MayNghien.Infrastructures.Models.Responses;
using Microsoft.Extensions.Logging;
using Todo.Domain.Entities;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.TodoLists;
using Todo.Application.Mapping;

namespace Todo.Application.Implementations.TodoLists
{
    public class CreateTodoListHandler : ICreateTodoListHandler
    {
        private readonly ITodoListRepository _todoListRepository;
        private readonly IUserService _userService;
        private readonly ILogger<CreateTodoListHandler> _logger;

        public CreateTodoListHandler(ITodoListRepository todoListRepository, IUserService userService, ILogger<CreateTodoListHandler> logger)
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

                var entity = new TodoList
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                };
                entity.SetCreatedInfo(user.Id);
                await _todoListRepository.AddAsync(entity);

                var response = TodoListMapper.ToResponse(entity);
                return result.BuildResult(response, "Todo list created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TodoList: {Name} create failed.", request.Name);
                return result.BuildError("An error occurred while creating the todo list.");
            }
        }
    }
}
