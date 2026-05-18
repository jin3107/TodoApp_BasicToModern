using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Models.Entities;

namespace Todo.Models.Configurations
{
    public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
    {
        public void Configure(EntityTypeBuilder<OtpCode> builder)
        {
            builder.ToTable("OtpCodes");
            builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(6);
            builder.Property(x => x.Purpose).HasConversion<string>();
        }
    }
}
