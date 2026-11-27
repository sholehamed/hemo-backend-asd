using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoReactionEventConfiguration : IEntityTypeConfiguration<HemoReactionEvent>
{
    public void Configure(EntityTypeBuilder<HemoReactionEvent> builder)
    {
        builder.ToTable("HemoReactionEvent");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Action).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.PreviousStatus).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.NewStatus).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Notes).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.EvidenceJson).IsUnicode();
        builder.HasIndex(x=>new { x.HemoReactionGUID, x.OccurredAt });
        builder.HasOne(x=>x.Reaction).WithMany().HasForeignKey(x=>x.HemoReactionGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
