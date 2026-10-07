using Microsoft.EntityFrameworkCore;
using Todo.Models.Data;

namespace Todo.API.Extensions
{
    public static class MigrationExtensions
    {
        private const int MaxAttempts = 10;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Applies pending EF Core migrations on startup. Retries because the database
        /// container may still be initializing when the API starts.
        /// </summary>
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    await db.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied");
                    return;
                }
                catch (Exception ex) when (attempt < MaxAttempts)
                {
                    logger.LogWarning(ex, "Migration attempt {Attempt}/{Max} failed, retrying", attempt, MaxAttempts);
                    await Task.Delay(RetryDelay);
                }
            }
        }
    }
}
