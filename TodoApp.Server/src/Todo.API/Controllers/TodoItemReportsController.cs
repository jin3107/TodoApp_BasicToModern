using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Todo.DTOs.Requests;
using Todo.Services.Interfaces;

namespace Todo.API.Controllers
{
    [Route("reports")]
    [ApiController]
    public class TodoItemReportsController : ControllerBase
    {
        private readonly ITodoItemReportService _todoItemReportService;

        public TodoItemReportsController(ITodoItemReportService todoItemReportService)
        {
            _todoItemReportService = todoItemReportService;
        }

        [HttpPost]
        [Route("progress")]
        public async Task<IActionResult> GetProgressReport([FromBody] TodoItemReportRequest request)
        {
            var result = await _todoItemReportService.GetProgressReportAsync(request);
            return Ok(result);
        }

        [HttpPost]
        [Route("snapshot")]
        public async Task<IActionResult> CreateDailySnapshot()
        {
            var result = await _todoItemReportService.CreateDailySnapshotAsync();
            return Ok(result);
        }
    }
}
