using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoQualityLineConfiguration : IEntityTypeConfiguration<HemoQualityLine>
{
    public void Configure(EntityTypeBuilder<HemoQualityLine> builder)
    {
        builder.ToTable("HemoQualityLine");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Title).IsUnicode().HasMaxLength(300).IsRequired();
        builder.Property(x=>x.DetailsJson).IsUnicode().IsRequired();
        builder.Property(x=>x.Status).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Evidence).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.Version).IsRowVersion();
        builder.HasIndex(x=>new { x.HemoQualityRecordGUID, x.Status });
        builder.HasIndex(x=>new { x.OwnerUserGUID, x.Status });
        builder.HasOne(x=>x.Record).WithMany().HasForeignKey(x=>x.HemoQualityRecordGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
