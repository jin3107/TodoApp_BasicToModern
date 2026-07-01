using LinqKit;
using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Todo.DTOs.Responses;
using Todo.Domain.Entities;
using Todo.Repositories.Interfaces;
using Todo.Application.Interfaces.TodoLists;
using Todo.Application.Mapping;
using static MayNghien.Infrastructures.Helpers.SearchHelper;
using Todo.Application.Interfaces;

namespace Todo.Application.Implementations.TodoLists
{
    public class SearchTodoListHandler : ISearchTodoListHandler
    {
        private readonly ITodoListRepository _todoListRepository;
        private readonly IUserService _userService;

        public SearchTodoListHandler(ITodoListRepository todoListRepository, IUserService userService)
        {
            _todoListRepository = todoListRepository;
            _userService = userService;
        }

        public async Task<AppResponse<SearchResponse<TodoListResponse>>> HandleAsync(SearchRequest request)
        {
            var result = new AppResponse<SearchResponse<TodoListResponse>>();
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var isSuperAdmin = user.Roles.Contains("SuperAdmin");

                var query = BuildFilterExpression(request.Filters!, isSuperAdmin ? null : user.Email);
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

        private static ExpressionStarter<TodoList> BuildFilterExpression(List<Filter> filters, string? ownerEmail)
        {
            try
            {
                var predicate = PredicateBuilder.New<TodoList>(true);
                if (ownerEmail != null)
                    predicate = predicate.And(x => x.CreatedBy == ownerEmail);

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
