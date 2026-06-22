using LinqKit;
using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Todo.DTOs.Responses;
using Todo.Domain.Entities;
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
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public SearchTodoListHandler(ITodoListRepository todoListRepository, 
            IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager)
        {
            _todoListRepository = todoListRepository;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        private async Task<ApplicationUser?> GetCurrentUser()
        => await _userManager.FindByEmailAsync(_contextAccessor.HttpContext?.User.Identity?.Name!);

        public async Task<AppResponse<SearchResponse<TodoListResponse>>> HandleAsync(SearchRequest request)
        {
            var result = new AppResponse<SearchResponse<TodoListResponse>>();
            try
            {
                var user = await GetCurrentUser();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var roles = await _userManager.GetRolesAsync(user);
                var isSuperAdmin = roles.Contains("SuperAdmin");

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

        private ExpressionStarter<TodoList> BuildFilterExpression(List<Filter> filters, string? ownerEmail)
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
