
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ssoConfiguration : IEntityTypeConfiguration<sso>
{
    public void Configure(EntityTypeBuilder<sso> builder)
    {
        builder.HasKey(u=>u.GUID);
        


        builder.HasIndex(u=>u.UserGUID);


        builder.HasIndex(u=>u.PackageGUID);


        builder.HasIndex(u=>u.OrgLocationGUID);


        builder.HasIndex(u=>u.Key).IsUnique();


        builder.Property(t => t.UserGUID).IsRequired();


        builder.Property(t => t.PackageGUID).IsRequired();


        builder.Property(t => t.OrgLocationGUID).IsRequired();


        builder.Property(t => t.IPAddress).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.UserAgent).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.ExpireDate).IsRequired();


        builder.Property(t => t.Key).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Sign).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.AdminLogin).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

