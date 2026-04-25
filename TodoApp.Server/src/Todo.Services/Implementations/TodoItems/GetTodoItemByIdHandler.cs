using MayNghien.Infrastructures.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Responses;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoItems;
using Todo.Services.Mapping;

namespace Todo.Services.Implementations.TodoItems
{
    public class GetTodoItemByIdHandler : IGetTodoItemByIdHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public GetTodoItemByIdHandler(ITodoItemRepository todoItemRepository)
        {
            _todoItemRepository = todoItemRepository;
        }

        public async Task<AppResponse<TodoItemResponse>> HandleAsync(Guid id)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
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
