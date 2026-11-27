using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class RequestClinicalDataConfiguration : IEntityTypeConfiguration<RequestClinicalData>
{
    public void Configure(EntityTypeBuilder<RequestClinicalData> builder)
    {
        builder.ToTable("RequestClinicalData");
        builder.HasKey(x=>x.GUID);
        builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Diagnosis).IsUnicode().HasMaxLength(500);
        builder.Property(x=>x.ClinicalIndication).IsUnicode().HasMaxLength(2000);
        builder.Property(x=>x.Hemoglobin).HasPrecision(8,2);
        builder.Property(x=>x.Hematocrit).HasPrecision(8,2);
        builder.Property(x=>x.Premedication).IsUnicode().HasMaxLength(500);
        builder.Property(x=>x.SampleStatus).IsUnicode().HasMaxLength(32);
        builder.Property(x=>x.Notes).IsUnicode().HasMaxLength(2000);
        builder.HasIndex(x=>x.BloodRequestGUID).IsUnique();
        builder.HasOne(x=>x.BloodRequest).WithOne(x=>x.ClinicalData).HasForeignKey<RequestClinicalData>(x=>x.BloodRequestGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
