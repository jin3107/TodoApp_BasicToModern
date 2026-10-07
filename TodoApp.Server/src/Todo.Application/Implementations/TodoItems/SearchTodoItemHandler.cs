using LinqKit;
using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<SearchTodoItemHandler> _logger;

        public SearchTodoItemHandler(ITodoItemRepository todoItemRepository, IUserService userService, ILogger<SearchTodoItemHandler> logger)
        {
            _todoItemRepository = todoItemRepository;
            _userService = userService;
            _logger = logger;
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

                var query = BuildFilterExpression(request.Filters!, isSuperAdmin ? null : user.Id);

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
                _logger.LogError(ex, "Failed to search TodoItems");
                result.BuildError("An error occurred while searching items.");
            }
            return result;
        }

        private ExpressionStarter<TodoItem> BuildFilterExpression(List<Filter> filters, string? ownerId)
        {
            try
            {
                var predicate = PredicateBuilder.New<TodoItem>(true);
                if (ownerId != null)
                    predicate = predicate.And(x => x.CreatedBy == ownerId);

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
                _logger.LogError(ex, "Failed to build TodoItem filter expression");
                throw;
            }
        }
    }
}
