using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoQualityRecordConfiguration : IEntityTypeConfiguration<HemoQualityRecord>
{
    public void Configure(EntityTypeBuilder<HemoQualityRecord> builder)
    {
        builder.ToTable("HemoQualityRecord");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Kind).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Number).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.Title).IsUnicode().HasMaxLength(300).IsRequired();
        builder.Property(x=>x.Ward).IsUnicode().HasMaxLength(120).IsRequired();
        builder.Property(x=>x.Status).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.DetailsJson).IsUnicode().IsRequired();
        builder.Property(x=>x.ReviewNotes).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.Version).IsRowVersion();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.Number }).IsUnique();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.Kind, x.Status, x.OccurredAt });
        builder.HasOne(x=>x.OrgLocation).WithMany().HasForeignKey(x=>x.OrgLocationGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
