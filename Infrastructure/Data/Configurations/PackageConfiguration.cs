
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.HasKey(u=>u.GUID);
        builder.HasQueryFilter(u => !u.IsDelete);


        builder.HasIndex(u=>u.PackageCategoryGUID);


        builder.HasIndex(u=>u.RoleGUID);


        builder.HasIndex(u=>u.DeletedBy);


        builder.Property(t => t.PackageCategoryGUID).IsRequired();


        builder.Property(t => t.Name).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Subtitle).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Description).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Price).IsRequired();


        builder.Property(t => t.Link).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.RoleGUID).IsRequired();


        builder.Property(t => t.Icon).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Gorder).IsRequired();


        builder.Property(t => t.ExpireDays).IsRequired();


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

