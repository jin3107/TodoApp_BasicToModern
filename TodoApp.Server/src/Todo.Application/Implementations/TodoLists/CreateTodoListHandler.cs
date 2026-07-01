using MayNghien.Infrastructures.Models.Responses;
using Todo.Domain.Entities;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Repositories.Interfaces;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.TodoLists;
using Todo.Application.Mapping;

namespace Todo.Application.Implementations.TodoLists
{
    public class CreateTodoListHandler : ICreateTodoListHandler
    {
        private readonly ITodoListRepository _todoListRepository;
        private readonly IUserService _userService;

        public CreateTodoListHandler(ITodoListRepository todoListRepository, IUserService userService)
        {
            _todoListRepository = todoListRepository;
            _userService = userService;
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
                entity.SetCreatedInfo(user.Email);
                await _todoListRepository.AddAsync(entity);

                var response = TodoListMapper.ToResponse(entity);
                return result.BuildResult(response, "Todo list created successfully.");
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
