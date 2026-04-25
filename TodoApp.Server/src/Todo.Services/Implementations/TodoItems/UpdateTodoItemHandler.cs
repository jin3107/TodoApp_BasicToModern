using MayNghien.Infrastructures.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoItems;
using Todo.Services.Mapping;

namespace Todo.Services.Implementations.TodoItems
{
    public class UpdateTodoItemHandler : IUpdateTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public UpdateTodoItemHandler(ITodoItemRepository todoItemRepository)
        {
            _todoItemRepository = todoItemRepository;
        }

        public async Task<AppResponse<TodoItemResponse>> HandleAsync(TodoItemRequest request)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                var task = await _todoItemRepository.GetAsync(request.Id);
                if (task == null || task.IsDeleted == true)
                    return result.BuildError("Item not found or deleted.");

                task.Title = request.Title;
                task.Description = request.Description;
                task.DueDate = request.DueDate;
                task.ModifiedOn = DateTime.UtcNow;
                task.IsCompleted = request.IsCompleted;
                task.Priority = request.Priority;
                task.CompletedOn = request.IsCompleted ? request.CompletedOn ?? DateTime.UtcNow : null;
                await _todoItemRepository.EditAsync(task);

                var response = TodoItemMapper.ToResponse(task);
                return result.BuildResult(response, "Item updated successfully.");
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
