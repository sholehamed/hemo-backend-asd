using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patient");
        builder.HasKey(x=>x.GUID);
        builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.IdentificationType).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.IdentificationNumber).IsUnicode().HasMaxLength(64).IsRequired();
        builder.Property(x=>x.FirstName).IsUnicode().HasMaxLength(100).IsRequired();
        builder.Property(x=>x.LastName).IsUnicode().HasMaxLength(100).IsRequired();
        builder.Property(x=>x.FatherName).IsUnicode().HasMaxLength(100);
        builder.Property(x=>x.Gender).IsUnicode().HasMaxLength(16).IsRequired();
        builder.Property(x=>x.Mobile).IsUnicode().HasMaxLength(32);
        builder.HasIndex(x=>x.OrgLocationGUID);
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.IdentificationType, x.IdentificationNumber, x.IsDelete }).IsUnique();
        builder.HasOne(x=>x.OrgLocation).WithMany().HasForeignKey(x=>x.OrgLocationGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
