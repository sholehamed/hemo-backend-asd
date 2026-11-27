
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class SubSystemConfiguration : IEntityTypeConfiguration<SubSystem>
{
    public void Configure(EntityTypeBuilder<SubSystem> builder)
    {
        builder.HasKey(u=>u.GUID);
        


        builder.Property(t => t.Name).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Host).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Port).IsRequired();


        builder.Property(t => t.Active).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

