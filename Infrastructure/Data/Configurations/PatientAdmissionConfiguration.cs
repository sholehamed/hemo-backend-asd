using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class PatientAdmissionConfiguration : IEntityTypeConfiguration<PatientAdmission>
{
    public void Configure(EntityTypeBuilder<PatientAdmission> builder)
    {
        builder.ToTable("PatientAdmission");
        builder.HasKey(x=>x.GUID);
        builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.HISNumber).IsUnicode().HasMaxLength(64).IsRequired();
        builder.Property(x=>x.Ward).IsUnicode().HasMaxLength(100).IsRequired();
        builder.Property(x=>x.BedNumber).IsUnicode().HasMaxLength(64).IsRequired();
        builder.Property(x=>x.Physician).IsUnicode().HasMaxLength(100).IsRequired();
        builder.Property(x=>x.AdmissionType).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Diagnosis).IsUnicode().HasMaxLength(500);
        builder.Property(x=>x.Insurance).IsUnicode().HasMaxLength(100);
        builder.Property(x=>x.BloodGroup).IsUnicode().HasMaxLength(8);
        builder.Property(x=>x.TransfusionHistory).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Notes).IsUnicode().HasMaxLength(2000);
        builder.Property(x=>x.IdentityStatus).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.WristbandCode).IsUnicode().HasMaxLength(64).IsRequired();
        builder.Property(x=>x.Status).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.ClearanceNotes).IsUnicode().HasMaxLength(2000);
        builder.Property(x=>x.Version).IsRowVersion();
        builder.HasIndex(x=>x.PatientGUID);
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.HISNumber, x.IsDelete }).IsUnique();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.AdmittedAt });
        builder.HasIndex(x=>x.WristbandCode).IsUnique();
        builder.HasOne(x=>x.Patient).WithMany(x=>x.Admissions).HasForeignKey(x=>x.PatientGUID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x=>x.OrgLocation).WithMany().HasForeignKey(x=>x.OrgLocationGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
