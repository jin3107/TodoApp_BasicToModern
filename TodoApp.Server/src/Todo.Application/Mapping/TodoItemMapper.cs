using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Domain.Entities;

namespace Todo.Application.Mapping
{
    public static class TodoItemMapper
    {
        public static TodoItemResponse ToResponse(TodoItem entity)
        {
            return new TodoItemResponse
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                DueDate = entity.DueDate,
                IsCompleted = entity.IsCompleted,
                Priority = entity.Priority,
                CompletedOn = entity.CompletedOn,
                CreatedOn = entity.CreatedOn,
                ModifiedOn = entity.ModifiedOn,
                TodoListId = entity.TodoListId
            };
        }
    }
}
