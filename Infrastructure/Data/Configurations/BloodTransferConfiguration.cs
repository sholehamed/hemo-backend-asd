using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class BloodTransferConfiguration : IEntityTypeConfiguration<BloodTransfer>
{
    public void Configure(EntityTypeBuilder<BloodTransfer> builder)
    {
        builder.ToTable("BloodTransfer");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.TransferNumber).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.PatientName).IsUnicode().HasMaxLength(200).IsRequired();
        builder.Property(x=>x.HisNumber).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.Ward).IsUnicode().HasMaxLength(120).IsRequired();
        builder.Property(x=>x.WristbandCode).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.UnitCode).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.ProductCode).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.BloodGroup).IsUnicode().HasMaxLength(8).IsRequired();
        builder.Property(x=>x.RequestVersion).HasMaxLength(8);
        builder.Property(x=>x.AdmissionVersion).HasMaxLength(8);
        builder.Property(x=>x.Status).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.CarrierName).IsUnicode().HasMaxLength(160).IsRequired();
        builder.Property(x=>x.CarrierCode).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.ContainerCode).IsUnicode().HasMaxLength(100).IsRequired();
        builder.Property(x=>x.DispatchTemperature).HasPrecision(9,2);
        builder.Property(x=>x.TransportIndicator).IsUnicode().HasMaxLength(100).IsRequired();
        builder.Property(x=>x.Version).IsRowVersion();
        builder.Property(x=>x.VolumeMl).HasPrecision(9,2);
        builder.HasIndex(x=>x.BankBloodUnitGUID).IsUnique();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.TransferNumber }).IsUnique();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.Status, x.DispatchedAt });
        builder.HasIndex(x=>x.BloodRequestGUID);
        builder.HasIndex(x=>x.PatientAdmissionGUID);
        builder.HasIndex(x=>x.BankCompatibilityTestGUID);
        builder.HasOne(x=>x.OrgLocation).WithMany().HasForeignKey(x=>x.OrgLocationGUID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x=>x.Unit).WithMany().HasForeignKey(x=>x.BankBloodUnitGUID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x=>x.BloodRequest).WithMany().HasForeignKey(x=>x.BloodRequestGUID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x=>x.Admission).WithMany().HasForeignKey(x=>x.PatientAdmissionGUID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x=>x.CompatibilityTest).WithMany().HasForeignKey(x=>x.BankCompatibilityTestGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
