using LinqKit;
using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoItems;
using Todo.Services.Mapping;
using static MayNghien.Infrastructures.Helpers.SearchHelper;

namespace Todo.Services.Implementations.TodoItems
{
    public class SearchTodoItemHandler : ISearchTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public SearchTodoItemHandler(ITodoItemRepository todoItemRepository)
        {
            _todoItemRepository = todoItemRepository;
        }

        public async Task<AppResponse<SearchResponse<TodoItemResponse>>> HandleAsync(SearchRequest request)
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

                            case "TodoListId":
                                if (!string.IsNullOrEmpty(filter.Value) && Guid.TryParse(filter.Value, out var todoListId))
                                    predicate = predicate.And(x => x.TodoListId == todoListId);
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
    }
}
