
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OrgLocationTypeConfiguration : IEntityTypeConfiguration<OrgLocationType>
{
    public void Configure(EntityTypeBuilder<OrgLocationType> builder)
    {
        builder.HasKey(u=>u.GUID);
        


        builder.Property(t => t.Name).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

