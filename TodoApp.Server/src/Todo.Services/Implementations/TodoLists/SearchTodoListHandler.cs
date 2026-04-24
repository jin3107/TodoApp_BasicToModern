using LinqKit;
using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Todo.DTOs.Responses;
using Todo.Models.Entities;
using Todo.Repositories.Interfaces;
using Todo.Services.Interfaces.TodoLists;
using Todo.Services.Mapping;
using static MayNghien.Infrastructures.Helpers.SearchHelper;

namespace Todo.Services.Implementations.TodoLists
{
    public class SearchTodoListHandler : ISearchTodoListHandler
    {
        private readonly ITodoListRepository _todoListRepository;

        public SearchTodoListHandler(ITodoListRepository todoListRepository)
        {
            _todoListRepository = todoListRepository;
        }

        public async Task<AppResponse<SearchResponse<TodoListResponse>>> HandleAsync(SearchRequest request)
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

                return result.BuildResult(searchResponse);
            }
            catch (Exception ex)
            {
                return result.BuildError(ex.Message + " " + ex.StackTrace);
            }
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
    }
}
