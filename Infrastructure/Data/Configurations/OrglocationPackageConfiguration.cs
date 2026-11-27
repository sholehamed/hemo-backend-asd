
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OrglocationPackageConfiguration : IEntityTypeConfiguration<OrglocationPackage>
{
    public void Configure(EntityTypeBuilder<OrglocationPackage> builder)
    {
        builder.HasKey(u=>u.GUID);
        builder.HasQueryFilter(u => !u.IsDelete);


        builder.HasIndex(u=>u.OrgLocationGUID);


        builder.HasIndex(u=>u.PackageGUID);


        builder.HasIndex(u=>u.DeletedBy);


        builder.Property(t => t.OrgLocationGUID).IsRequired();


        builder.Property(t => t.PackageGUID).IsRequired();


        builder.Property(t => t.ExpiryDate).IsRequired();


        builder.Property(t => t.Hidden).IsRequired();


        builder.Property(t => t.Deleted);


        builder.Property(t => t.DeletedBy);


        builder.Property(t => t.DeletedByIP).IsUnicode().HasMaxLength(256);


        builder.Property(t => t.IsDelete).IsRequired();


        builder.Property(t => t.LastModified).IsRequired();


        builder.Property(t => t.LastModifiedBy).IsRequired();


        builder.Property(t => t.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.CreatedBy).IsRequired();


        builder.Property(t => t.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

