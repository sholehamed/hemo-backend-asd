using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoEquipmentConfiguration : IEntityTypeConfiguration<HemoEquipment>
{
    public void Configure(EntityTypeBuilder<HemoEquipment> builder)
    {
        builder.ToTable("HemoEquipment");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Code).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.Name).IsUnicode().HasMaxLength(200).IsRequired();
        builder.Property(x=>x.Category).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.BrandModel).IsUnicode().HasMaxLength(200).IsRequired();
        builder.Property(x=>x.Ward).IsUnicode().HasMaxLength(120).IsRequired();
        builder.Property(x=>x.SerialNumber).IsUnicode().HasMaxLength(100);
        builder.Property(x=>x.Status).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Version).IsRowVersion();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.Code }).IsUnique();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.Status });
        builder.HasOne(x=>x.OrgLocation).WithMany().HasForeignKey(x=>x.OrgLocationGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
