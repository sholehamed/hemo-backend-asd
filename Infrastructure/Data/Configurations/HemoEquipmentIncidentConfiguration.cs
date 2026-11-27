using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoEquipmentIncidentConfiguration : IEntityTypeConfiguration<HemoEquipmentIncident>
{
    public void Configure(EntityTypeBuilder<HemoEquipmentIncident> builder)
    {
        builder.ToTable("HemoEquipmentIncident");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.IncidentNumber).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.Urgency).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Status).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Description).IsUnicode().HasMaxLength(4000).IsRequired();
        builder.Property(x=>x.Resolution).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.ResolutionReference).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.Version).IsRowVersion();
        builder.HasIndex(x=>x.IncidentNumber).IsUnique();
        builder.HasIndex(x=>new { x.HemoEquipmentGUID, x.Status });
        builder.HasOne(x=>x.Equipment).WithMany().HasForeignKey(x=>x.HemoEquipmentGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
