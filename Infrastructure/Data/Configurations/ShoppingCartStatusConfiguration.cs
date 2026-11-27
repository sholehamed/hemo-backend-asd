
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ShoppingCartStatusConfiguration : IEntityTypeConfiguration<ShoppingCartStatus>
{
    public void Configure(EntityTypeBuilder<ShoppingCartStatus> builder)
    {
        builder.HasKey(u=>u.GUID);
        


        builder.HasIndex(u=>u.Gkey).IsUnique();


        builder.Property(t => t.Name).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Gkey).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

