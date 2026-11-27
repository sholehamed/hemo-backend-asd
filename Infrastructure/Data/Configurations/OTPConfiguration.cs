
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OTPConfiguration : IEntityTypeConfiguration<OTP>
{
    public void Configure(EntityTypeBuilder<OTP> builder)
    {
        builder.HasKey(u=>u.GUID);
        


        builder.HasIndex(u=>u.Username);


        builder.Property(t => t.Username).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Code).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.ExDate).IsRequired();


        builder.Property(t => t.TryCount).IsRequired();


        builder.Property(t => t.Used).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

