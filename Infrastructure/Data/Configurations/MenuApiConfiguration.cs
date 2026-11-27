
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MenuApiConfiguration : IEntityTypeConfiguration<MenuApi>
{
    public void Configure(EntityTypeBuilder<MenuApi> builder)
    {
        builder.HasKey(u=>u.GUID);
        


        builder.HasIndex(u=>u.MenuGUID);


        builder.HasIndex(u=>u.ApiGUID);


        builder.Property(t => t.MenuGUID).IsRequired();


        builder.Property(t => t.ApiGUID).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

