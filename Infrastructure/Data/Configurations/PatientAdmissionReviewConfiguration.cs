using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class PatientAdmissionReviewConfiguration : IEntityTypeConfiguration<PatientAdmissionReview>
{
    public void Configure(EntityTypeBuilder<PatientAdmissionReview> builder)
    {
        builder.ToTable("PatientAdmissionReview");
        builder.HasKey(x=>x.GUID);
        builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Kind).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Outcome).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.Notes).IsUnicode().HasMaxLength(2000);
        builder.HasIndex(x=>new { x.PatientAdmissionGUID, x.ReviewedAt });
        builder.HasOne(x=>x.PatientAdmission).WithMany(x=>x.Reviews).HasForeignKey(x=>x.PatientAdmissionGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
