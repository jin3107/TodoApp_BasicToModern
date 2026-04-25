using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoLists;
using Todo.Services.Mapping;

namespace Todo.Services.Implementations.TodoLists
{
    public class CreateTodoListHandler : ICreateTodoListHandler
    {
        private readonly ITodoListRepository _todoListRepository;

        public CreateTodoListHandler(ITodoListRepository todoListRepository)
        {
            _todoListRepository = todoListRepository;
        }

        public async Task<AppResponse<TodoListResponse>> HandleAsync(TodoListRequest request)
        {
            var result = new AppResponse<TodoListResponse>();
            try
            {
                var entity = TodoListMapper.ToEntity(request);
                entity.Id = Guid.NewGuid();
                entity.Name = request.Name;
                entity.Description = request.Description;
                entity.CreatedBy = "admin";
                entity.CreatedOn = DateTime.UtcNow;
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
