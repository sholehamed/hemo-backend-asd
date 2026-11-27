using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class BloodRequestItemConfiguration : IEntityTypeConfiguration<BloodRequestItem>
{
    public void Configure(EntityTypeBuilder<BloodRequestItem> builder)
    {
        builder.ToTable("BloodRequestItem");
        builder.HasKey(x=>x.GUID);
        builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.ProductCode).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.RequestedBloodGroup).IsUnicode().HasMaxLength(8);
        builder.Property(x=>x.SpecialInstructions).IsUnicode().HasMaxLength(1000);
        builder.HasIndex(x=>new { x.BloodRequestGUID, x.ProductCode, x.IsDelete }).IsUnique();
        builder.HasOne(x=>x.BloodRequest).WithMany(x=>x.Items).HasForeignKey(x=>x.BloodRequestGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
