using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Models.Entities;

namespace Todo.Services.Mapping
{
    public static class TodoListMapper
    {
        public static TodoList ToEntity(TodoListRequest request)
        {
            return new TodoList
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description
            };
        }

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