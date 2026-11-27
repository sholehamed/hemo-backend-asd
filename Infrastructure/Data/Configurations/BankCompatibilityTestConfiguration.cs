using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class BankCompatibilityTestConfiguration : IEntityTypeConfiguration<BankCompatibilityTest>
{
    public void Configure(EntityTypeBuilder<BankCompatibilityTest> builder)
    {
        builder.ToTable("BankCompatibilityTest");
        builder.HasKey(x=>x.GUID);
        builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.RequestVersion).HasMaxLength(8);
        builder.Property(x=>x.AdmissionVersion).HasMaxLength(8);
        builder.Property(x=>x.SampleReference).IsUnicode().HasMaxLength(100).IsRequired();
        builder.Property(x=>x.SecondSample).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.LabelGroup).IsUnicode().HasMaxLength(8).IsRequired();
        builder.Property(x=>x.Method).IsUnicode().HasMaxLength(100).IsRequired();
        builder.Property(x=>x.ImmediateSpin).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.Phase37).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.Ahg).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.Dat).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.AntibodyScreen).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.ReportReference).IsUnicode().HasMaxLength(100).IsRequired();
        builder.Property(x=>x.Conclusion).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.Notes).IsUnicode().HasMaxLength(2000);
        builder.HasIndex(x=>new { x.BankBloodUnitGUID, x.BloodRequestGUID, x.RecordedAt });
        builder.HasIndex(x=>x.BloodRequestGUID);
        builder.HasOne(x=>x.Unit).WithMany().HasForeignKey(x=>x.BankBloodUnitGUID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x=>x.BloodRequest).WithMany().HasForeignKey(x=>x.BloodRequestGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
