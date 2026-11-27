using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class BloodTransferVitalConfiguration : IEntityTypeConfiguration<BloodTransferVital>
{
    public void Configure(EntityTypeBuilder<BloodTransferVital> builder)
    {
        builder.ToTable("BloodTransferVital");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Phase).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.Temperature).HasPrecision(9,2);
        builder.Property(x=>x.UrineMl).HasPrecision(9,2);
        builder.Property(x=>x.Notes).IsUnicode().HasMaxLength(2000);
        builder.Property(x=>x.RateMlPerHour).HasPrecision(9,2);
        builder.HasIndex(x=>new { x.BloodTransferGUID, x.MeasuredAt });
        builder.HasIndex(x=>new { x.BloodTransferGUID, x.Phase });
        builder.HasOne(x=>x.Transfer).WithMany().HasForeignKey(x=>x.BloodTransferGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
