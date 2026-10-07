using MayNghien.Infrastructures.Models.Responses;
using Microsoft.Extensions.Logging;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.TodoLists;

namespace Todo.Application.Implementations.TodoLists
{
    public class DeleteTodoListHandler : IDeleteTodoListHandler
    {
        private readonly ITodoListRepository _todoListRepository;
        private readonly IUserService _userService;
        private readonly ILogger<DeleteTodoListHandler> _logger;

        public DeleteTodoListHandler(ITodoListRepository todoListRepository, IUserService userService, ILogger<DeleteTodoListHandler> logger)
        {
            _todoListRepository = todoListRepository;
            _userService = userService;
            _logger = logger;
        }

        public async Task<AppResponse<string>> HandleAsync(Guid id)
        {
            var result = new AppResponse<string>();
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var entity = await _todoListRepository.GetByIdAsync(id);
                if (entity == null || entity.IsDeleted == true)
                    return result.BuildError("Todo list not found or deleted.");

                if (!user.Roles.Contains("SuperAdmin") && entity.CreatedBy != user.Id)
                    return result.BuildError("Forbidden: you do not own this resource.");

                entity.IsDeleted = true;
                await _todoListRepository.UpdateAsync(entity);

                return result.BuildResult("Todo list deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TodoListId: {Id} delete failed.", id);
                return result.BuildError("An error occurred while deleting the todo list.");
            }
        }
    }
}
