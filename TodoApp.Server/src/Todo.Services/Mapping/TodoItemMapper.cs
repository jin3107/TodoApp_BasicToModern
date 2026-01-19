using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Models.Entities;

namespace Todo.Services.Mapping
{
    public static class TodoItemMapper
    {
        public static TodoItem ToEntity(TodoItemRequest request)
        {
            return new TodoItem
            {
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                IsCompleted = request.IsCompleted,
                Priority = request.Priority,
                CompletedOn = request.CompletedOn,
                TodoListId = request.TodoListId,
            };
        }

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
