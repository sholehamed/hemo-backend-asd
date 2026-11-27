
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(u=>u.GUID);
        builder.HasQueryFilter(u => !u.IsDelete);


        builder.HasIndex(u=>u.OrgLocationTypeGUID);


        builder.HasIndex(u=>u.PRoleGUID);


        builder.HasIndex(u=>u.OrgLocationGUID);


        builder.HasIndex(u=>u.Path).IsUnique();


        builder.HasIndex(u=>u.Gkey).IsUnique();


        builder.HasIndex(u=>u.Level);


        builder.HasIndex(u=>u.DeletedBy);


        builder.Property(t => t.OrgLocationTypeGUID);


        builder.Property(t => t.PRoleGUID);


        builder.Property(t => t.Name).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.OrgLocationGUID);


        builder.Property(t => t.Path).IsUnicode().HasMaxLength(128).IsRequired();


        builder.Property(t => t.Gkey).IsRequired();


        builder.Property(t => t.Level).IsRequired();


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

