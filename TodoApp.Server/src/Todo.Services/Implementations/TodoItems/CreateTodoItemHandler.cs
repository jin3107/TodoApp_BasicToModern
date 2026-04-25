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
    public class CreateTodoItemHandler : ICreateTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public CreateTodoItemHandler(ITodoItemRepository todoItemRepository)
        {
            _todoItemRepository = todoItemRepository;
        }

        public async Task<AppResponse<TodoItemResponse>> HandleAsync(TodoItemRequest request)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                var newTask = TodoItemMapper.ToEntity(request);
                newTask.Id = Guid.NewGuid();
                newTask.Title = request.Title;
                newTask.Description = request.Description;
                newTask.DueDate = request.DueDate;
                newTask.Priority = request.Priority;
                newTask.TodoListId = request.TodoListId;
                newTask.CreatedOn = DateTime.UtcNow;
                newTask.IsCompleted = false;
                newTask.CompletedOn = null;
                await _todoItemRepository.AddAsync(newTask);

                var response = TodoItemMapper.ToResponse(newTask);
                return result.BuildResult(response, "Item created successfully.");
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
        }
    }
}
