using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Responses;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.TodoLists;
using Todo.Application.Mapping;

namespace Todo.Application.Implementations.TodoLists
{
    public class GetTodoListByIdHandler : IGetTodoListByIdHandler
    {
        private readonly ITodoListRepository _todoListRepository;
        private readonly IUserService _userService;

        public GetTodoListByIdHandler(ITodoListRepository todoListRepository, IUserService userService)
        {
            _todoListRepository = todoListRepository;
            _userService = userService;
        }

        public async Task<AppResponse<TodoListResponse>> HandleAsync(Guid id)
        {
            var result = new AppResponse<TodoListResponse>();
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var entity = await _todoListRepository.GetByIdAsync(id);
                if (entity == null || entity.IsDeleted == true)
                    return result.BuildError("Todo list not found or deleted.");

                var response = TodoListMapper.ToResponse(entity);
                return result.BuildResult(response);
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
