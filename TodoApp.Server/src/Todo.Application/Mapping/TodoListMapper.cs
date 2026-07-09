using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Domain.Entities;

namespace Todo.Application.Mapping
{
    public static class TodoListMapper
    {
        public static TodoListResponse ToResponse(TodoList entity)
        {
            var response = new TodoListResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CreatedOn = entity.CreatedOn,
                ModifiedOn = entity.ModifiedOn
            };

            if (entity.TodoItems != null)
            {
                response.TotalItems = entity.TodoItems.Count;
                response.CompletedItems = entity.TodoItems.Count(x => x.IsCompleted);
            }

            return response;
        }
    }
}
