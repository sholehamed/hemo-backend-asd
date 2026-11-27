
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class RoleAccessConfiguration : IEntityTypeConfiguration<RoleAccess>
{
    public void Configure(EntityTypeBuilder<RoleAccess> builder)
    {
        builder.HasKey(u=>u.GUID);
        builder.HasQueryFilter(u => !u.IsDelete);


        builder.HasIndex(u=>u.MenuGUID);


        builder.HasIndex(u=>u.RoleGUID);


        builder.HasIndex(u=>u.ApiGUID);


        builder.HasIndex(u=>u.PRoleAccessGUID);


        builder.HasIndex(u=>u.OrglocationPackageGUID);


        builder.HasIndex(u=>u.Path).IsUnique();


        builder.HasIndex(u=>u.Level);


        builder.HasIndex(u=>u.Gkey).IsUnique();


        builder.HasIndex(u=>u.DeletedBy);


        builder.Property(t => t.MenuGUID).IsRequired();


        builder.Property(t => t.RoleGUID).IsRequired();


        builder.Property(t => t.ApiGUID);


        builder.Property(t => t.PRoleAccessGUID);


        builder.Property(t => t.OrglocationPackageGUID);


        builder.Property(t => t.Path).IsUnicode().HasMaxLength(128).IsRequired();


        builder.Property(t => t.Level).IsRequired();


        builder.Property(t => t.Gkey).IsRequired();


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

