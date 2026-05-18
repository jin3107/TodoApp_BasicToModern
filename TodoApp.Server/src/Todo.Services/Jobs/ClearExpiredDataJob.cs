using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Services.Interfaces.Background;

namespace Todo.Services.Jobs
{
    public class ClearExpiredDataJob : IJob
    {
        private readonly IClearExpiredDataHandler _handler;
        private readonly ILogger<ClearExpiredDataJob> _logger;

        public ClearExpiredDataJob(IClearExpiredDataHandler handler, 
            ILogger<ClearExpiredDataJob> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try { await _handler.HandleAsync(); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClearExpiredDataJob failed");
                throw new JobExecutionException(ex, refireImmediately: false);
            }
        }
    }
}
