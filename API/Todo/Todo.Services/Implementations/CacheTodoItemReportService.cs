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
    public class CachedTodoItemReportService : ITodoItemReportService
    {
        private readonly ILogger<CachedTodoItemReportService> _logger;
        private readonly TodoItemReportService _reportService;
        private readonly ICacheService _cacheService;
        private const string REPORT_CACHE_KEY_PREFIX = "report:progress:";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public CachedTodoItemReportService(TodoItemReportService reportService, ICacheService cacheService, ILogger<CachedTodoItemReportService> logger)
        {
            _reportService = reportService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<AppResponse<Guid>> CreateDailySnapshotAsync()
        {
            var result = new AppResponse<Guid>();
            try
            {
                result = await _reportService.CreateDailySnapshotAsync();
                if (result.IsSuccess)
                {
                    await _cacheService.RemoveByPatternAsync($"{REPORT_CACHE_KEY_PREFIX}*");
                    _logger.LogInformation("Created daily snapshot successfully and cleared report cache");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating daily snapshot");
                return result.BuildError($"An error occurred while creating daily snapshot: {ex.Message}");
            }

            return result;
        }

        public async Task<AppResponse<TodoItemReportResponse>> GetProgressReportAsync(TodoItemReportRequest request)
        {
            var result = new AppResponse<TodoItemReportResponse>();
            try
            {
                var cacheKey = GenerateReportCacheKey(request);
                var cachedReport = await _cacheService.GetAsync<AppResponse<TodoItemReportResponse>>(cacheKey);
                if (cachedReport != null)
                {
                    _logger.LogInformation("Cache HIT: Progress Report with key {cacheKey}", cacheKey);
                    return cachedReport;
                }

                _logger.LogWarning("Cache MISS: Progress Report with key {cacheKey}", cacheKey);
                result = await _reportService.GetProgressReportAsync(request);
                if (result.IsSuccess && result.Data != null)
                {
                    await _cacheService.SetAsync(cacheKey, result, _cacheExpiration);
                    _logger.LogInformation("Cached get progress result for key {CacheKey}", cacheKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting progress report");
                return result.BuildError($"An error occurred while retrieving progress report: {ex.Message}");
            }

            return result;
        }

        private string GenerateReportCacheKey(TodoItemReportRequest request)
        {
            var keyBuilder = new StringBuilder(REPORT_CACHE_KEY_PREFIX);
            var startDate = request.StartDate?.ToString("dd-MM-yyyy") ?? "all";
            var endDate = request.EndDate?.ToString("dd-MM-yyyy") ?? "all";

            keyBuilder.Append($"start:{startDate}:");
            keyBuilder.Append($"end:{endDate}");

            return keyBuilder.ToString();
        }
    }
}
