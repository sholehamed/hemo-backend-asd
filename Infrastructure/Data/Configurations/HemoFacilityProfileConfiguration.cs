using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoFacilityProfileConfiguration : IEntityTypeConfiguration<HemoFacilityProfile>
{
    public void Configure(EntityTypeBuilder<HemoFacilityProfile> builder)
    {
        builder.ToTable("HemoFacilityProfile");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.FacilityName).IsUnicode().HasMaxLength(200).IsRequired();
        builder.Property(x=>x.AccreditationCode).IsUnicode().HasMaxLength(100);
        builder.Property(x=>x.Chair).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.Secretary).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.Officer).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.BloodCenter).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.BloodCenterCode).IsUnicode().HasMaxLength(100);
        builder.Property(x=>x.ReportFooter).IsUnicode().HasMaxLength(1000);
        builder.Property(x=>x.Version).IsRowVersion();
        builder.HasIndex(x=>x.OrgLocationGUID).IsUnique();
        builder.HasOne(x=>x.OrgLocation).WithMany().HasForeignKey(x=>x.OrgLocationGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
