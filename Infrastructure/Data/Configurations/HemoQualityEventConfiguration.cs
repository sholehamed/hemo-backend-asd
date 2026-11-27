using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoQualityEventConfiguration : IEntityTypeConfiguration<HemoQualityEvent>
{
    public void Configure(EntityTypeBuilder<HemoQualityEvent> builder)
    {
        builder.ToTable("HemoQualityEvent");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Action).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.PreviousStatus).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.NewStatus).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Notes).IsUnicode().HasMaxLength(4000).IsRequired();
        builder.Property(x=>x.EvidenceJson).IsUnicode();
        builder.HasIndex(x=>new { x.HemoQualityRecordGUID, x.OccurredAt });
        builder.HasOne(x=>x.Record).WithMany().HasForeignKey(x=>x.HemoQualityRecordGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
