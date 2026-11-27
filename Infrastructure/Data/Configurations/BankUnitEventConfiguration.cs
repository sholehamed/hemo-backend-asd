using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class BankUnitEventConfiguration : IEntityTypeConfiguration<BankUnitEvent>
{
    public void Configure(EntityTypeBuilder<BankUnitEvent> builder)
    {
        builder.ToTable("BankUnitEvent");
        builder.HasKey(x=>x.GUID);
        builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Action).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.PreviousStatus).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.NewStatus).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.Reason).IsUnicode().HasMaxLength(2000);
        builder.Property(x=>x.Method).IsUnicode().HasMaxLength(100);
        builder.Property(x=>x.Temperature).HasPrecision(7,2);
        builder.HasIndex(x=>new { x.BankBloodUnitGUID, x.OccurredAt });
        builder.HasIndex(x=>x.BloodRequestGUID);
        builder.HasOne(x=>x.Unit).WithMany().HasForeignKey(x=>x.BankBloodUnitGUID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x=>x.BloodRequest).WithMany().HasForeignKey(x=>x.BloodRequestGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
