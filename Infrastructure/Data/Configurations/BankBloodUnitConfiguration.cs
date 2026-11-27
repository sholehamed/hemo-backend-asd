using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class BankBloodUnitConfiguration : IEntityTypeConfiguration<BankBloodUnit>
{
    public void Configure(EntityTypeBuilder<BankBloodUnit> builder)
    {
        builder.ToTable("BankBloodUnit");
        builder.HasKey(x=>x.GUID);
        builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.UnitCode).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.ProductCode).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.BloodGroup).IsUnicode().HasMaxLength(8).IsRequired();
        builder.Property(x=>x.BatchNumber).IsUnicode().HasMaxLength(80);
        builder.Property(x=>x.SupplySource).IsUnicode().HasMaxLength(200).IsRequired();
        builder.Property(x=>x.EquipmentCode).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.Status).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.Version).IsRowVersion();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.UnitCode }).IsUnique();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.Status, x.ExpiresAt });
        builder.HasIndex(x=>new { x.BloodRequestGUID, x.Status });
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.EquipmentCode });
        builder.HasOne(x=>x.OrgLocation).WithMany().HasForeignKey(x=>x.OrgLocationGUID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x=>x.BloodRequest).WithMany().HasForeignKey(x=>x.BloodRequestGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
