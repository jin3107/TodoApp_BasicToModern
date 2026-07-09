using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Domain.Entities;

namespace Todo.Models.Configurations
{
    public class TodoItemProgressReportConfiguration : IEntityTypeConfiguration<TodoItemProgressReport>
    {
        public void Configure(EntityTypeBuilder<TodoItemProgressReport> builder)
        {
            builder.ToTable("TodoItemProgressReports");
            builder.Property(x => x.CompletionRate).HasColumnType("decimal(5,2)");
            builder.Property(x => x.AverageCompletionTimeHours).HasColumnType("decimal(10,2)");
            builder.Property(x => x.Notes).HasMaxLength(2000);
        }
    }
}
