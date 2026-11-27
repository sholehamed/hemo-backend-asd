using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoFacilityProfileEventConfiguration : IEntityTypeConfiguration<HemoFacilityProfileEvent>
{
    public void Configure(EntityTypeBuilder<HemoFacilityProfileEvent> builder)
    {
        builder.ToTable("HemoFacilityProfileEvent");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Notes).IsUnicode().HasMaxLength(4000).IsRequired();
        builder.Property(x=>x.PreviousJson).IsUnicode();
        builder.Property(x=>x.CurrentJson).IsUnicode().IsRequired();
        builder.HasIndex(x=>new { x.HemoFacilityProfileGUID, x.OccurredAt });
        builder.HasOne(x=>x.Profile).WithMany().HasForeignKey(x=>x.HemoFacilityProfileGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
