using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class BloodTransferEventConfiguration : IEntityTypeConfiguration<BloodTransferEvent>
{
    public void Configure(EntityTypeBuilder<BloodTransferEvent> builder)
    {
        builder.ToTable("BloodTransferEvent");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Action).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.PreviousStatus).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.NewStatus).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.Notes).IsUnicode().HasMaxLength(2000);
        builder.Property(x=>x.EvidenceJson).IsUnicode();
        builder.HasIndex(x=>new { x.BloodTransferGUID, x.OccurredAt });
        builder.HasOne(x=>x.Transfer).WithMany().HasForeignKey(x=>x.BloodTransferGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
