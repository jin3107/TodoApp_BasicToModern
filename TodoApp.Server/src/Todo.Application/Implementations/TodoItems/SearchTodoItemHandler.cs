using LinqKit;
using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Todo.DTOs.Responses;
using Todo.Domain.Entities;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.TodoItems;
using Todo.Application.Mapping;
using static MayNghien.Infrastructures.Helpers.SearchHelper;
using Todo.Application.Interfaces;

namespace Todo.Application.Implementations.TodoItems
{
    public class SearchTodoItemHandler : ISearchTodoItemHandler
    {
        private readonly ITodoItemRepository _todoItemRepository;
        private readonly IUserService _userService;

        public SearchTodoItemHandler(ITodoItemRepository todoItemRepository, IUserService userService)
        {
            _todoItemRepository = todoItemRepository;
            _userService = userService;
        }

        public async Task<AppResponse<SearchResponse<TodoItemResponse>>> HandleAsync(SearchRequest request)
        {
            var result = new AppResponse<SearchResponse<TodoItemResponse>>();
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return result.BuildError("Unauthorized.");

                var isSuperAdmin = user.Roles.Contains("SuperAdmin");

                var query = BuildFilterExpression(request.Filters!, isSuperAdmin ? null : user.Email);

                int pageIndex = request.PageIndex ?? 1;
                int pageSize = request.PageSize ?? 10;
                var page = await _todoItemRepository.SearchAsync(query, request.SortBy, pageIndex, pageSize);
                var dtoList = page.Items.Select(TodoItemMapper.ToResponse).ToList();
                var searchResponse = new SearchResponse<TodoItemResponse>
                {
                    TotalPages = CalculateNumOfPages(page.TotalCount, pageSize),
                    TotalRows = page.TotalCount,
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

        private ExpressionStarter<TodoItem> BuildFilterExpression(List<Filter> filters, string? ownerEmail)
        {
            try
            {
                var predicate = PredicateBuilder.New<TodoItem>(true);
                if (ownerEmail != null)
                    predicate = predicate.And(x => x.CreatedBy == ownerEmail);

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
