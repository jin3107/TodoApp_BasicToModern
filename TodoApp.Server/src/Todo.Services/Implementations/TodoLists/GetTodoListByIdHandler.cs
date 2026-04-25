using MayNghien.Infrastructures.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Todo.DTOs.Responses;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoLists;
using Todo.Services.Mapping;

namespace Todo.Services.Implementations.TodoLists
{
    public class GetTodoListByIdHandler : IGetTodoListByIdHandler
    {
        private readonly ITodoListRepository _todoListRepository;

        public GetTodoListByIdHandler(ITodoListRepository todoListRepository)
        {
            _todoListRepository = todoListRepository;
        }

        public async Task<AppResponse<TodoListResponse>> HandleAsync(Guid id)
        {
            var result = new AppResponse<TodoListResponse>();
            try
            {
                var entity = await _todoListRepository.FindByAsync(p => p.Id == id).FirstOrDefaultAsync();
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
