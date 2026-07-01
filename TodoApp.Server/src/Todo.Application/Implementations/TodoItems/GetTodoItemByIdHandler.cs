using MayNghien.Infrastructures.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Todo.DTOs.Responses;
using Todo.Repositories.Interfaces;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.TodoItems;
using Todo.Application.Mapping;

namespace Todo.Application.Implementations.TodoItems
{
    public class GetTodoItemByIdHandler : IGetTodoItemByIdHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IUserService _userService;

        public GetTodoItemByIdHandler(ITodoItemRepository todoItemRepository, IUserService userService)
        {
            _todoItemRepository = todoItemRepository;
            _userService = userService;
        }

        public async Task<AppResponse<TodoItemResponse>> HandleAsync(Guid id)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return result.BuildError("Unauthorized");

                var task = await _todoItemRepository.FindByAsync(p => p.Id == id).FirstOrDefaultAsync();
                if (task == null || task.IsDeleted == true)
                    return result.BuildError("Item not found or deleted.");

                var response = TodoItemMapper.ToResponse(task);
                return result.BuildResult(response);
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
