using MayNghien.Infrastructures.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Todo.Models.Entities;
using TodoItem = Todo.Models.Entities.TodoItem;

namespace Todo.Models.Data
{
    public class ApplicationDbContext : BaseContext<ApplicationUser>
    {
        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<TodoList> TodoLists { get; set; }
        public DbSet<TodoItemProgressReport> TaskProgressReports { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var appSetting = JsonConvert.DeserializeObject<AppSetting>(File.ReadAllText("appsettings.json"));
                optionsBuilder.UseMySql(appSetting!.ConnectionString,
                    new MySqlServerVersion(new Version(8, 0, 44)),
                    mySQLOptions =>
                    {
                        mySQLOptions.CommandTimeout(300);
                        mySQLOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(300),
                            errorNumbersToAdd: null
                        );
                    });
            }
        }
    }
}
