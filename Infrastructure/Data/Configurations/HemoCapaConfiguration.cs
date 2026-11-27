using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoCapaConfiguration : IEntityTypeConfiguration<HemoCapa>
{
    public void Configure(EntityTypeBuilder<HemoCapa> builder)
    {
        builder.ToTable("HemoCapa");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Title).IsUnicode().HasMaxLength(300).IsRequired();
        builder.Property(x=>x.Kind).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.OwnerName).IsUnicode().HasMaxLength(200).IsRequired();
        builder.Property(x=>x.Indicator).IsUnicode().HasMaxLength(1000).IsRequired();
        builder.Property(x=>x.Status).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Evidence).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.VerificationNotes).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.Version).IsRowVersion();
        builder.HasIndex(x=>new { x.HemoReactionGUID, x.Status });
        builder.HasIndex(x=>new { x.OwnerUserGUID, x.Status, x.DueAt });
        builder.HasOne(x=>x.Reaction).WithMany().HasForeignKey(x=>x.HemoReactionGUID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x=>x.Owner).WithMany().HasForeignKey(x=>x.OwnerUserGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
