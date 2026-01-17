using Azure.Core;
using LinqKit;
using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces;
using Todo.Services.Mapping;
using static MayNghien.Infrastructures.Helpers.SearchHelper;

namespace Todo.Services.Implementations
{
    public class TodoItemService : ITodoItemService
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public TodoItemService(ITodoItemRepository todoItemRepository)
        {
            _todoItemRepository = todoItemRepository;
        }

        public async Task<AppResponse<TodoItemResponse>> CreateAsync(TodoItemRequest request)
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
                newTask.CreatedOn = DateTime.UtcNow;
                newTask.IsCompleted = false;
                newTask.CompletedOn = null;
                await _todoItemRepository.AddAsync(newTask);

                var response = TodoItemMapper.ToResponse(newTask);
                result.BuildResult(response, "Item created successfully.");
            }
            catch (Exception ex)
            {
                result.BuildError(ex.Message + " " + ex.StackTrace);
            }
            return result;
        }

        public async Task<AppResponse<string>> DeleteAsync(Guid id)
        {
            var result = new AppResponse<string>();
            try
            {
                var task = await _todoItemRepository.GetAsync(id);
                if (task == null || task.IsDeleted == true)
                    return result.BuildError("Item not found or deleted.");
                task.IsDeleted = true;
                await _todoItemRepository.EditAsync(task);
                result.BuildResult("Item deleted successfully.");
            }
            catch (Exception ex)
            {
                result.BuildError(ex.Message + " " + ex.StackTrace);
            }
            return result;
        }

        public async Task<AppResponse<TodoItemResponse>> GetByIdAsync(Guid id)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                var task = await _todoItemRepository.FindByAsync(p => p.Id == id).FirstOrDefaultAsync();
                if (task == null || task.IsDeleted == true)
                    return result.BuildError("Item not found or deleted.");

                var response = TodoItemMapper.ToResponse(task);
                result.BuildResult(response);
            }
            catch (Exception ex)
            {
                result.BuildError(ex.Message + " " + ex.StackTrace);
            }
            return result;
        }

        public async Task<AppResponse<SearchResponse<TodoItemResponse>>> SearchAsync(SearchRequest request)
        {
            var result = new AppResponse<SearchResponse<TodoItemResponse>>();
            try
            {
                var query = BuildFilterExpression(request.Filters!);
                var numOfRecords = await _todoItemRepository.CountRecordsAsync(query);
                var tasks = _todoItemRepository.FindByPredicate(query).AsQueryable();

                if (request.SortBy != null)
                    tasks = _todoItemRepository.AddSort(tasks, request.SortBy);
                else
                    tasks = tasks.OrderBy(x => x.Title);

                int pageIndex = request.PageIndex ?? 1;
                int pageSize = request.PageSize ?? 10;
                int startIndex = (pageIndex - 1) * pageSize;
                var classList = await tasks.Skip(startIndex).Take(pageSize).ToListAsync();
                var dtoList = classList.Select(TodoItemMapper.ToResponse).ToList();
                var searchResponse = new SearchResponse<TodoItemResponse>
                {
                    TotalPages = CalculateNumOfPages(numOfRecords, pageSize),
                    TotalRows = numOfRecords,
                    CurrentPage = pageIndex,
                    Data = dtoList,
                    RowsPerPage = pageSize,
                };
                result.Data = searchResponse;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.BuildError(ex.Message + " " + ex.StackTrace);
            }
            return result;
        }

        private ExpressionStarter<TodoItem> BuildFilterExpression(List<Filter> filters)
        {
            try
            {
                var predicate = PredicateBuilder.New<TodoItem>(true);
                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        switch (filter.FieldName)
                        {
                            case "Title":
                                if (!string.IsNullOrEmpty(filter.Value))
                                    predicate = predicate.And(x => x.Title.Contains(filter.Value));
                                break;

                            default: break;
                        }
                    }
                }

                predicate = predicate.And(x => x.IsDeleted == false);
                return predicate;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " " + ex.StackTrace);
            }
        }

        public async Task<AppResponse<TodoItemResponse>> UpdateAsync(TodoItemRequest request)
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
                result.BuildResult(response, "Item updated successfully.");
            }
            catch (Exception ex)
            {
                result.BuildError(ex.Message + " " + ex.StackTrace);
            }
            return result;
        }
    }
}
