using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class BloodRequestConfiguration : IEntityTypeConfiguration<BloodRequest>
{
    public void Configure(EntityTypeBuilder<BloodRequest> builder)
    {
        builder.ToTable("BloodRequest");
        builder.HasKey(x=>x.GUID);
        builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.RequestNumber).IsUnicode().HasMaxLength(64).IsRequired();
        builder.Property(x=>x.Source).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.FormType).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Priority).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.Status).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.AdmissionVersionAtSubmission).HasMaxLength(8);
        builder.Property(x=>x.Version).IsRowVersion();
        builder.HasIndex(x=>new { x.PatientAdmissionGUID, x.Status });
        builder.HasIndex(x=>x.ClientRequestId).IsUnique();
        builder.HasIndex(x=>x.RequestNumber).IsUnique();
        builder.HasIndex(x=>x.RequesterUserGUID);
        builder.HasIndex(x=>new { x.Status, x.SubmittedAt });
        builder.HasOne(x=>x.PatientAdmission).WithMany().HasForeignKey(x=>x.PatientAdmissionGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
