
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PasswordBlockListConfiguration : IEntityTypeConfiguration<PasswordBlockList>
{
    public void Configure(EntityTypeBuilder<PasswordBlockList> builder)
    {
        builder.HasKey(u=>u.GUID);
        


        builder.HasIndex(u=>u.Password);


        builder.Property(t => t.Password).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

