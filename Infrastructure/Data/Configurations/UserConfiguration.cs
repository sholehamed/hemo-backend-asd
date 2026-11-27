
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u=>u.GUID);
        builder.HasQueryFilter(u => !u.IsDelete);


        builder.HasIndex(u=>u.Username).IsUnique();


        builder.HasIndex(u=>u.Email).IsUnique();


        builder.HasIndex(u=>u.DeletedBy);


        builder.Property(t => t.Username).IsUnicode().HasMaxLength(64);


        builder.Property(t => t.Password).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.FirstName).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.LastName).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Mobile).IsUnicode().HasMaxLength(16);


        builder.Property(t => t.Email).IsUnicode().HasMaxLength(64);


        builder.Property(t => t.Nationalcode).IsUnicode().HasMaxLength(10);


        builder.Property(t => t.Blocked).IsRequired();


        builder.Property(t => t.Locked).IsRequired();


        builder.Property(t => t.LockoutEndDate);


        builder.Property(t => t.FailedLoginCount).IsRequired();


        builder.Property(t => t.ForceChangePassword).IsRequired();


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

