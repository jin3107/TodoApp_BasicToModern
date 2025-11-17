using MayNghien.Infrastructures.Models.Requests;
using MayNghien.Infrastructures.Models.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.DTOs.Requests;
using Todo.DTOs.Responses;
using Todo.Services.Interfaces;

namespace Todo.Services.Implementations
{
    public class CacheTodoItemService : ITodoItemService
    {
        private readonly TodoItemService _todoItemService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheTodoItemService> _logger;
        private const string TODOITEM_CACHE_KEY_PREFIX = "todo-item:";
        private const string TODOITEM_SEARCH_CACHE_KEY_PREFIX = "todo-item:search:";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public CacheTodoItemService(TodoItemService todoItemService, ICacheService cacheService, 
            ILogger<CacheTodoItemService> logger)
        {
            _todoItemService = todoItemService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<AppResponse<TodoItemResponse>> CreateAsync(TodoItemRequest request)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                result = await _todoItemService.CreateAsync(request);
                if (result.IsSuccess)
                {
                    await _cacheService.RemoveByPatternAsync($"{TODOITEM_SEARCH_CACHE_KEY_PREFIX}*");
                    _logger.LogInformation("Created Todo-item successfully and cleared search cache");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating Todo-item");
                return result.BuildError($"An error occurred while creating the todo item: {ex.Message}");
            }

            return result;
        }

        public async Task<AppResponse<string>> DeleteAsync(Guid id)
        {
            var result = new AppResponse<string>();
            try
            {
                result = await _todoItemService.DeleteAsync(id);
                if (result.IsSuccess)
                {
                    await _cacheService.RemoveAsync($"{TODOITEM_CACHE_KEY_PREFIX}{id}");
                    await _cacheService.RemoveByPatternAsync($"{TODOITEM_SEARCH_CACHE_KEY_PREFIX}*");
                    _logger.LogInformation("Deleted Todo-item {id} and cleared related cache", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting Todo-item {id}", id);
                return result.BuildError($"An error occurred while deleting the todo item: {ex.Message}");
            }

            return result;
        }

        public async Task<AppResponse<TodoItemResponse>> GetByIdAsync(Guid id)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                var cacheKey = $"{TODOITEM_CACHE_KEY_PREFIX}{id}";
                //var cachedTask = await _cacheService.GetAsync<AppResponse<TodoItemResponse>>(cacheKey);
                var cachedData = await _cacheService.GetAsync<TodoItemResponse>(cacheKey);
                if (cachedData != null)
                {
                    _logger.LogInformation("Cache HIT: Todo-item {id} (Source: {CacheType})", id, _cacheService.GetType().Name);
                    return result.BuildResult(cachedData);
                }

                _logger.LogWarning("Cache MISS: Todo-item {id} - Querying database...", id);
                result = await _todoItemService.GetByIdAsync(id);
                if (result.IsSuccess && result.Data != null)
                {
                    await _cacheService.SetAsync(cacheKey, result.Data, _cacheExpiration);
                    _logger.LogInformation("Cached Todo-item {id} for {ExpirationMinutes} minutes", id, _cacheExpiration.TotalMinutes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting Todo-item {id}", id);
                return result.BuildError($"An error occurred while retrieving the todo item: {ex.Message}");
            }

            return result;
        }

        public async Task<AppResponse<SearchResponse<TodoItemResponse>>> SearchAsync(SearchRequest request)
       {
            var result = new AppResponse<SearchResponse<TodoItemResponse>>();
            try
            {
                var cacheKey = GenerateSearchCacheKey(request);
                //var cachedResult = await _cacheService.GetAsync<AppResponse<SearchResponse<TodoItemResponse>>>(cacheKey);
                var cachedData = await _cacheService.GetAsync<SearchResponse<TodoItemResponse>>(cacheKey);
                if (cachedData != null)
                {
                    _logger.LogInformation("Cache hit: Search with key {cacheKey}", cacheKey);
                    return result.BuildResult(cachedData);
                }

                _logger.LogWarning("Cache miss: Search with key {cacheKey}", cacheKey);
                result = await _todoItemService.SearchAsync(request);
                if (result.IsSuccess && result != null)
                {
                    await _cacheService.SetAsync(cacheKey, result.Data, _cacheExpiration);
                    _logger.LogInformation("Cached search result for key {CacheKey}", cacheKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while searching Todo-items");
                return result.BuildError($"An error occurred while searching todo items: {ex.Message}");
            }

            return result;
        }

        private string GenerateSearchCacheKey(SearchRequest request)
        {
            var keyBuilder = new StringBuilder(TODOITEM_SEARCH_CACHE_KEY_PREFIX);
            keyBuilder.Append($"page:{request.PageIndex ?? 1}:");
            keyBuilder.Append($"size:{request.PageSize ?? 10}:");

            var sortKey = request.SortBy != null
                ? $"{request.SortBy.FieldName}:{request.SortBy.Ascending}"
                : "default";
            keyBuilder.Append($"sort:{sortKey}:");
            if (request.Filters != null && request.Filters.Any())
            {
                foreach (var filter in request.Filters)
                {
                    keyBuilder.Append($"{filter.FieldName}:{filter.Value}:");
                }
            }

            return keyBuilder.ToString();
        }

        public async Task<AppResponse<TodoItemResponse>> UpdateAsync(TodoItemRequest request)
        {
            var result = new AppResponse<TodoItemResponse>();
            try
            {
                result = await _todoItemService.UpdateAsync(request);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Updated Todo-item {id} successfully", request.Id);
                    await _cacheService.RemoveAsync($"{TODOITEM_CACHE_KEY_PREFIX}{request.Id}");
                    await _cacheService.RemoveByPatternAsync($"{TODOITEM_SEARCH_CACHE_KEY_PREFIX}*");
                    _logger.LogInformation("Cache cleared for Todo-item {TaskId} and related search results", request.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to update Todo-item {id}: {Message}", request.Id, result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating Todo-item {id}", request.Id);
                return result.BuildError($"An error occurred while updating the todo item: {ex.Message}");
            }
            return result;
        }
    }
}
