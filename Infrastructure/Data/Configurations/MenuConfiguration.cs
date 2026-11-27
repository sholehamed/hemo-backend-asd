
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.HasKey(u=>u.GUID);
        


        builder.HasIndex(u=>u.PMenuGUID);


        builder.HasIndex(u=>u.routerLink).IsUnique();


        builder.HasIndex(u=>u.Path).IsUnique();


        builder.HasIndex(u=>u.Level);


        builder.HasIndex(u=>u.Gkey).IsUnique();


        builder.Property(t => t.PMenuGUID);


        builder.Property(t => t.label).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.icon).IsUnicode().HasMaxLength(256);


        builder.Property(t => t.routerLink).IsUnicode().HasMaxLength(256);


        builder.Property(t => t.Gorder).IsRequired();


        builder.Property(t => t.ShowInMenu).IsRequired();


        builder.Property(t => t.IsPublic).IsRequired();


        builder.Property(t => t.Path).IsUnicode().HasMaxLength(128).IsRequired();


        builder.Property(t => t.Level).IsRequired();


        builder.Property(t => t.Gkey).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

