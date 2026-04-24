using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Todo.DTOs.Requests;
using Todo.Services.Interfaces;
using Todo.Services.Interfaces.Reports;

namespace Todo.API.Controllers
{
    [Route("reports")]
    [ApiController]
    public class TodoItemReportsController : ControllerBase
    {
        private readonly IGetProgressReportHandler _getReport;
        private readonly ICreateDailySnapshotHandler _createSnapshot;

        public TodoItemReportsController(
            IGetProgressReportHandler getReport,
            ICreateDailySnapshotHandler createSnapshot)
        {
            _getReport = getReport;
            _createSnapshot = createSnapshot;
        }

        [HttpPost("progress")]
        public async Task<IActionResult> GetProgressReport([FromBody] TodoItemReportRequest request)
            => Ok(await _getReport.HandleAsync(request));

        [HttpPost("snapshot")]
        public async Task<IActionResult> CreateDailySnapshot()
            => Ok(await _createSnapshot.HandleAsync());
    }
}
