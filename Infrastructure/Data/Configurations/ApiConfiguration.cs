
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ApiConfiguration : IEntityTypeConfiguration<Api>
{
    public void Configure(EntityTypeBuilder<Api> builder)
    {
        builder.HasKey(u=>u.GUID);
        builder.HasQueryFilter(u => !u.IsDelete);


        builder.HasIndex(u=>u.SubSystemGUID);


        builder.HasIndex(u=>u.DeletedBy);


        builder.Property(t => t.DownstreamPathTemplate).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.UpstreamPathTemplate).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.IsPublic).IsRequired();


        builder.Property(t => t.Priority).IsRequired();


        builder.Property(t => t.SubSystemGUID).IsRequired();


        builder.Property(t => t.APITypeName).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Active).IsRequired();


        builder.Property(t => t.Title).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Internet).IsRequired();


        builder.Property(t => t.Intranet).IsRequired();


        builder.Property(t => t.LogData).IsRequired();


        builder.Property(t => t.Sensitive).IsUnicode().HasMaxLength(256);


        builder.Property(t => t.LogRequestHeaders).IsRequired();


        builder.Property(t => t.LogResponseHeaders).IsRequired();


        builder.Property(t => t.LogRequestBody).IsRequired();


        builder.Property(t => t.LogResponseBody).IsRequired();


        builder.Property(t => t.EnableRateLimiting).IsRequired();


        builder.Property(t => t.Period);


        builder.Property(t => t.Limit);


        builder.Property(t => t.Deleted);


        builder.Property(t => t.DeletedBy);


        builder.Property(t => t.DeletedByIP).IsUnicode().HasMaxLength(256);


        builder.Property(t => t.IsDelete).IsRequired();


        builder.Property(t => t.LastModified).IsRequired();


        builder.Property(t => t.LastModifiedBy).IsRequired();


        builder.Property(t => t.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.CreatedBy).IsRequired();


        builder.Property(t => t.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

