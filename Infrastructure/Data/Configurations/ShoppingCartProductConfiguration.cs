
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ShoppingCartProductConfiguration : IEntityTypeConfiguration<ShoppingCartProduct>
{
    public void Configure(EntityTypeBuilder<ShoppingCartProduct> builder)
    {
        builder.HasKey(u=>u.GUID);
        


        builder.HasIndex(u=>u.PackageGUID);


        builder.HasIndex(u=>u.ShoppingCartGUID);


        builder.HasIndex(u=>u.OrgLocationGUID);


        builder.Property(t => t.PackageGUID).IsRequired();


        builder.Property(t => t.ShoppingCartGUID).IsRequired();


        builder.Property(t => t.OrgLocationGUID).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

