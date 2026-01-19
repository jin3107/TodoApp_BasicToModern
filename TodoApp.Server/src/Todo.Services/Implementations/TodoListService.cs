using LinqKit;
using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class TodoListService : ITodoListService
    {
        private readonly ITodoListRepository _todoListRepository;

        public TodoListService(ITodoListRepository todoListRepository)
        {
            _todoListRepository = todoListRepository;
        }

        public async Task<AppResponse<TodoListResponse>> CreateAsync(TodoListRequest request)
        {
            var result = new AppResponse<TodoListResponse>();
            try
            {
                var entity = TodoListMapper.ToEntity(request);
                entity.Id = Guid.NewGuid();
                entity.Name = request.Name;
                entity.Description = request.Description;
                entity.CreatedOn = DateTime.UtcNow;
                await _todoListRepository.AddAsync(entity);

                var response = TodoListMapper.ToResponse(entity);
                result.BuildResult(response, "Todo list created successfully.");
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
                var entity = await _todoListRepository.GetAsync(id);
                if (entity == null || entity.IsDeleted == true)
                    return result.BuildError("Todo list not found or deleted.");
                entity.IsDeleted = true;
                await _todoListRepository.EditAsync(entity);
                result.BuildResult("Todo list deleted successfully.");
            }
            catch (Exception ex)
            {
                result.BuildError(ex.Message + " " + ex.StackTrace);
            }
            return result;
        }

        public async Task<AppResponse<TodoListResponse>> GetByIdAsync(Guid id)
        {
            var result = new AppResponse<TodoListResponse>();
            try
            {
                var entity = await _todoListRepository.FindByAsync(p => p.Id == id).FirstOrDefaultAsync();
                if (entity == null || entity.IsDeleted == true)
                    return result.BuildError("Todo list not found or deleted.");

                var response = TodoListMapper.ToResponse(entity);
                result.BuildResult(response);
            }
            catch (Exception ex)
            {
                result.BuildError(ex.Message + " " + ex.StackTrace);
            }
            return result;
        }

        public async Task<AppResponse<SearchResponse<TodoListResponse>>> SearchAsync(SearchRequest request)
        {
            var result = new AppResponse<SearchResponse<TodoListResponse>>();
            try
            {
                var query = BuildFilterExpression(request.Filters!);
                var numOfRecords = await _todoListRepository.CountRecordsAsync(query);
                var todoLists = _todoListRepository.FindByPredicate(query).AsQueryable();

                if (request.SortBy != null)
                    todoLists = _todoListRepository.AddSort(todoLists, request.SortBy);
                else
                    todoLists = todoLists.OrderBy(x => x.Name);

                int pageIndex = request.PageIndex ?? 1;
                int pageSize = request.PageSize ?? 10;
                int startIndex = (pageIndex - 1) * pageSize;
                var list = await todoLists.Skip(startIndex).Take(pageSize).ToListAsync();
                var dtoList = list.Select(TodoListMapper.ToResponse).ToList();
                var searchResponse = new SearchResponse<TodoListResponse>
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

        private ExpressionStarter<TodoList> BuildFilterExpression(List<Filter> filters)
        {
            try
            {
                var predicate = PredicateBuilder.New<TodoList>(true);
                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        switch (filter.FieldName)
                        {
                            case "Name":
                                if (!string.IsNullOrEmpty(filter.Value))
                                    predicate = predicate.And(x => x.Name.Contains(filter.Value));
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

        public async Task<AppResponse<TodoListResponse>> UpdateAsync(TodoListRequest request)
        {
            var result = new AppResponse<TodoListResponse>();
            try
            {
                var entity = await _todoListRepository.GetAsync(request.Id);
                if (entity == null || entity.IsDeleted == true)
                    return result.BuildError("Todo list not found or deleted.");

                entity.Name = request.Name;
                entity.Description = request.Description;
                entity.ModifiedOn = DateTime.UtcNow;
                await _todoListRepository.EditAsync(entity);
                var response = TodoListMapper.ToResponse(entity);
                result.BuildResult(response, "Todo list updated successfully.");
            }
            catch (Exception ex)
            {
                result.BuildError(ex.Message + " " + ex.StackTrace);
            }
            return result;
        }
    }
}