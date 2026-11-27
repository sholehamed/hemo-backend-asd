using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class BankTemperatureLogConfiguration : IEntityTypeConfiguration<BankTemperatureLog>
{
    public void Configure(EntityTypeBuilder<BankTemperatureLog> builder)
    {
        builder.ToTable("BankTemperatureLog");
        builder.HasKey(x=>x.GUID);
        builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.EquipmentCode).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.Temperature).HasPrecision(7,2);
        builder.Property(x=>x.Assessment).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.Notes).IsUnicode().HasMaxLength(2000);
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.EquipmentCode, x.MeasuredAt });
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.RecordedAt });
        builder.HasOne(x=>x.OrgLocation).WithMany().HasForeignKey(x=>x.OrgLocationGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
