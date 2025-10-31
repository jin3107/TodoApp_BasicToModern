using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Todo.DTOs.Requests;
using Todo.Services.Interfaces;

namespace Todo.API.Controllers
{
    [Route("reports")]
    [ApiController]
    public class TaskReportsController : ControllerBase
    {
        private readonly ITasksReportService _taskReportsService;

        public TaskReportsController(ITasksReportService taskReportsService)
        {
            _taskReportsService = taskReportsService;
        }

        [HttpPost]
        [Route("progress")]
        public async Task<IActionResult> GetProgressReport([FromBody] TaskReportRequest request)
        {
            var result = await _taskReportsService.GetProgressReportAsync(request);
            return Ok(result);
        }

        [HttpPost]
        [Route("snapshot")]
        public async Task<IActionResult> CreateDailySnapshot()
        {
            var result = await _taskReportsService.CreateDailySnapshotAsync();
            return Ok(result);
        }

        [HttpGet]
        [Route("trend")]
        public async Task<IActionResult> GetCompletionTrend([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _taskReportsService.GetCompletionTrendAsync(startDate, endDate);
            return Ok(result);
        }
    }
}
