using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace Todo.API.Controllers
{
    [Route("jobs")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IScheduler _scheduler;
        private readonly ILogger<JobsController> _logger;

        public JobsController(ISchedulerFactory schedulerFactory, ILogger<JobsController> logger)
        {
            _scheduler = schedulerFactory.GetScheduler().Result;
            _logger = logger;
        }

        [HttpPost("trigger/daily-report")]
        public async Task<IActionResult> TriggerDailyReport()
        {
            try
            {
                var jobKey = new JobKey("DailyTaskReportJob", "EmailJobs");
                await _scheduler.TriggerJob(jobKey);

                _logger.LogInformation("Daily report job triggered manually at {Time}", DateTime.Now);

                return Ok(new { Message = "Daily report job triggered successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger daily report job");
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
