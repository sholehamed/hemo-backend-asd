using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoReportConfiguration : IEntityTypeConfiguration<HemoReport>
{
    public void Configure(EntityTypeBuilder<HemoReport> builder)
    {
        builder.ToTable("HemoReport");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.ReportNumber).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.Title).IsUnicode().HasMaxLength(300).IsRequired();
        builder.Property(x=>x.Ward).IsUnicode().HasMaxLength(120);
        builder.Property(x=>x.ProductCode).IsUnicode().HasMaxLength(32);
        builder.Property(x=>x.SnapshotJson).IsUnicode().IsRequired();
        builder.Property(x=>x.ExternalStatus).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.ExternalReference).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.Version).IsRowVersion();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.ReportNumber }).IsUnique();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.PreparedAt });
        builder.HasOne(x=>x.OrgLocation).WithMany().HasForeignKey(x=>x.OrgLocationGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
