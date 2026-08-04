using MayNghien.Infrastructures.Models.Responses;
using Microsoft.Extensions.Logging;
using Todo.Application.Interfaces;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.TodoItems;

namespace Todo.Application.Implementations.TodoItems
{
    public class DeleteTodoItemHandler : IDeleteTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IUserService _userService;
        private readonly ILogger<DeleteTodoItemHandler> _logger;

        public DeleteTodoItemHandler(ITodoItemRepository todoItemRepository, IUserService userService, ILogger<DeleteTodoItemHandler> logger)
        {
            _todoItemRepository = todoItemRepository;
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

                var task = await _todoItemRepository.GetByIdAsync(id);
                if (task == null || task.IsDeleted == true)
                    return result.BuildError("Item not found or deleted.");

                if (!user.Roles.Contains("SuperAdmin") && task.CreatedBy != user.Id)
                    return result.BuildError("Forbidden: you do not own this resource.");

                task.IsDeleted = true;
                await _todoItemRepository.UpdateAsync(task);

                return result.BuildResult("Item deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ItemId: {id} delete failed.", id);
                return result.BuildError("An error occured while deleting the item.");
            }
        }
    }
}
