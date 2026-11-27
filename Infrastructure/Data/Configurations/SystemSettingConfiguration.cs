
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.HasKey(u=>u.GUID);
        builder.HasQueryFilter(u => !u.IsDelete);


        builder.HasIndex(u=>u.LoginApiGUID);


        builder.HasIndex(u=>u.DeletedBy);


        builder.Property(t => t.TokenExpireSeconds).IsRequired();


        builder.Property(t => t.RefreshTokenExpireSeconds).IsRequired();


        builder.Property(t => t.LoginAttempts).IsRequired();


        builder.Property(t => t.LockUserSeconds).IsRequired();


        builder.Property(t => t.LoginApiGUID).IsRequired();


        builder.Property(t => t.MultiLoginAlowed).IsRequired();


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

