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
using Todo.Services.Interfaces.Reports;

namespace Todo.Services.Implementations.Reports
{
    public class CachedGetProgressReportHandler : IGetProgressReportHandler
    {
        private readonly IGetProgressReportHandler _inner; 
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachedGetProgressReportHandler> _logger;
        private const string PREFIX = "report:progress:";
        private readonly TimeSpan _expiration = TimeSpan.FromMinutes(30);

        public CachedGetProgressReportHandler(IGetProgressReportHandler inner, ICacheService cacheService, ILogger<CachedGetProgressReportHandler> logger)
        {
            _inner = inner;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<AppResponse<TodoItemReportResponse>> HandleAsync(TodoItemReportRequest request)
        {
            var result = new AppResponse<TodoItemReportResponse>();
            try
            {
                var cacheKey = GenerateCacheKey(request);
                var cached = await _cacheService.GetAsync<AppResponse<TodoItemReportResponse>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Cache HIT: {CacheKey}", cacheKey);
                    return cached;
                }

                _logger.LogWarning("Cache MISS: {CacheKey}", cacheKey);
                result = await _inner.HandleAsync(request); 
                if (result.IsSuccess && result.Data != null)
                    await _cacheService.SetAsync(cacheKey, result, _expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress report");
                return result.BuildError($"An error occurred: {ex.Message}");
            }
            return result;
        }

        private string GenerateCacheKey(TodoItemReportRequest request)
        {
            var start = request.StartDate?.ToString("dd-MM-yyyy") ?? "all";
            var end = request.EndDate?.ToString("dd-MM-yyyy") ?? "all";
            return $"{PREFIX}start:{start}:end:{end}";
        }
    }
}
