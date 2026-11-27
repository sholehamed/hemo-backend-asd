using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoEquipmentEventConfiguration : IEntityTypeConfiguration<HemoEquipmentEvent>
{
    public void Configure(EntityTypeBuilder<HemoEquipmentEvent> builder)
    {
        builder.ToTable("HemoEquipmentEvent");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Kind).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.PreviousStatus).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.NewStatus).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Notes).IsUnicode().HasMaxLength(4000).IsRequired();
        builder.Property(x=>x.Reference).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.Provider).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.EvidenceJson).IsUnicode();
        builder.HasIndex(x=>new { x.HemoEquipmentGUID, x.OccurredAt });
        builder.HasOne(x=>x.Equipment).WithMany().HasForeignKey(x=>x.HemoEquipmentGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
