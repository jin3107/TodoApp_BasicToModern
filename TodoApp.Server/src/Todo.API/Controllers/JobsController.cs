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

        [HttpPost("trigger/weekly-summary")]
        public async Task<IActionResult> TriggerWeeklySummary()
        {
            try
            {
                var jobKey = new JobKey("WeeklyTaskSummaryJob", "EmailJobs");
                await _scheduler.TriggerJob(jobKey);

                _logger.LogInformation("Weekly summary job triggered manually at {Time}", DateTime.Now);

                return Ok(new { Message = "Weekly summary job triggered successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger weekly summary job");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("trigger/task-reminder")]
        public async Task<IActionResult> TriggerTaskReminder()
        {
            try
            {
                var jobKey = new JobKey("TaskReminderJob", "EmailJobs");
                await _scheduler.TriggerJob(jobKey);

                _logger.LogInformation("Task reminder job triggered manually at {Time}", DateTime.Now);

                return Ok(new { Message = "Task reminder job triggered successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger task reminder job");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("pause/{jobName}")]
        public async Task<IActionResult> PauseJob(string jobName)
        {
            try
            {
                var jobKey = new JobKey(jobName, "EmailJobs");
                await _scheduler.PauseJob(jobKey);

                return Ok(new { Message = $"Job '{jobName}' paused successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to pause job {JobName}", jobName);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("resume/{jobName}")]
        public async Task<IActionResult> ResumeJob(string jobName)
        {
            try
            {
                var jobKey = new JobKey(jobName, "EmailJobs");
                await _scheduler.ResumeJob(jobKey);

                return Ok(new { Message = $"Job '{jobName}' resumed successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resume job {JobName}", jobName);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("scheduler/info")]
        public async Task<IActionResult> GetSchedulerInfo()
        {
            var metadata = await _scheduler.GetMetaData();

            return Ok(new
            {
                SchedulerName = metadata.SchedulerName,
                SchedulerInstanceId = metadata.SchedulerInstanceId,
                IsStarted = metadata.Started,
                IsInStandbyMode = metadata.InStandbyMode,
                IsShutdown = metadata.Shutdown,
                JobsExecuted = metadata.NumberOfJobsExecuted,
                RunningSince = metadata.RunningSince?.LocalDateTime,
                SchedulerType = metadata.SchedulerType?.FullName
            });
        }
    }
}
