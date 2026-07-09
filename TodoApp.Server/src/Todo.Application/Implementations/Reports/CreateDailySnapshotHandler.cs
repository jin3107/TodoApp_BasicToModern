using MayNghien.Infrastructures.Models.Responses;
using Microsoft.Extensions.Logging;
using Todo.DTOs.Requests;
using Todo.Application.Interfaces.Cache;
using Todo.Application.Interfaces.Repositories;
using Todo.Application.Interfaces.Reports;
using Todo.Application.Mapping;

namespace Todo.Application.Implementations.Reports
{
    public class CreateDailySnapshotHandler : ICreateDailySnapshotHandler
    {
        private readonly IGetProgressReportHandler _getReport;
        private readonly ITodoItemProgressReportRepository _reportRepository;
        private readonly ICacheService _cacheService;
        private const string REPORT_CACHE_KEY_PREFIX = "report:progress:";
        private readonly ILogger<CreateDailySnapshotHandler> _logger;

        public CreateDailySnapshotHandler(
            IGetProgressReportHandler getReport,
            ITodoItemProgressReportRepository reportRepository,
            ICacheService cacheService,
            ILogger<CreateDailySnapshotHandler> logger)
        {
            _getReport = getReport;
            _reportRepository = reportRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<AppResponse<Guid>> HandleAsync()
        {
            var result = new AppResponse<Guid>();
            try
            {
                var reportResponse = await _getReport.HandleAsync(new TodoItemReportRequest());
                if (!reportResponse.IsSuccess || reportResponse.Data == null)
                    return result.BuildError("Failed to generate report for snapshot");

                var now = DateTime.UtcNow;
                var snapshot = TodoItemReportMapper.ToEntity(reportResponse.Data, now.Date, "Auto-generated daily snapshot");

                await _reportRepository.AddAsync(snapshot);

                await _cacheService.RemoveByPatternAsync($"{REPORT_CACHE_KEY_PREFIX}*");
                _logger.LogInformation("Created daily snapshot {Id} and cleared report cache", snapshot.Id);

                result.BuildResult(snapshot.Id, "Daily snapshot created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating daily snapshot");
                result.BuildError($"Error creating daily snapshot: {ex.Message}");
            }
            return result;
        }
    }
}
