
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OrgTypeConfiguration : IEntityTypeConfiguration<OrgType>
{
    public void Configure(EntityTypeBuilder<OrgType> builder)
    {
        builder.HasKey(u=>u.GUID);
        


        builder.HasIndex(u=>u.OrgLocationTypeGUID);


        builder.HasIndex(u=>u.OrgLocationGUID);


        builder.Property(t => t.OrgLocationTypeGUID).IsRequired();


        builder.Property(t => t.OrgLocationGUID).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

