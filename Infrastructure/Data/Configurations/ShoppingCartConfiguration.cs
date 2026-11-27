
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
{
    public void Configure(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.HasKey(u=>u.GUID);
        


        builder.HasIndex(u=>u.ShoppingCartStatusGUID);


        builder.HasIndex(u=>u.UserGUID);


        builder.HasIndex(u=>u.TransactionGUID);


        builder.Property(t => t.ShoppingCartStatusGUID).IsRequired();


        builder.Property(t => t.UserGUID).IsRequired();


        builder.Property(t => t.TransactionGUID);


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

