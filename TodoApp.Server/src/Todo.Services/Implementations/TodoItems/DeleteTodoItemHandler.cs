using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoItems;

namespace Todo.Services.Implementations.TodoItems
{
    public class DeleteTodoItemHandler : IDeleteTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public DeleteTodoItemHandler(ITodoItemRepository todoItemRepository)
        {
            _todoItemRepository = todoItemRepository;
        }

        public async Task<AppResponse<string>> HandleAsync(Guid id)
        {
            var result = new AppResponse<string>();
            try
            {
                var task = await _todoItemRepository.GetAsync(id);
                if (task == null || task.IsDeleted == true)
                    return result.BuildError("Item not found or deleted.");
                task.IsDeleted = true;
                await _todoItemRepository.EditAsync(task);
                return result.BuildResult("Item deleted successfully.");
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
