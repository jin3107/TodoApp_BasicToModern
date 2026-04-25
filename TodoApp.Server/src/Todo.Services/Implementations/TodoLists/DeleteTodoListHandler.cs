using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoLists;

namespace Todo.Services.Implementations.TodoLists
{
    public class DeleteTodoListHandler : IDeleteTodoListHandler
    {
        private readonly ITodoListRepository _todoListRepository;

        public DeleteTodoListHandler(ITodoListRepository todoListRepository)
        {
            _todoListRepository = todoListRepository;
        }

        public async Task<AppResponse<string>> HandleAsync(Guid id)
        {
            var result = new AppResponse<string>();
            try
            {
                var entity = await _todoListRepository.GetAsync(id);
                if (entity == null || entity.IsDeleted == true)
                    return result.BuildError("Todo list not found or deleted.");
                entity.IsDeleted = true;
                await _todoListRepository.EditAsync(entity);
                return result.BuildResult("Todo list deleted successfully.");
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
