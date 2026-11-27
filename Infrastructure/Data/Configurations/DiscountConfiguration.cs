
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.HasKey(u=>u.GUID);
        builder.HasQueryFilter(u => !u.IsDelete);


        builder.HasIndex(u=>u.Code).IsUnique();


        builder.HasIndex(u=>u.DeletedBy);


        builder.Property(t => t.Code).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Percentage).IsRequired();


        builder.Property(t => t.Value).IsRequired();


        builder.Property(t => t.MaxDiscountAmount);


        builder.Property(t => t.MinimumOrderAmount).IsRequired();


        builder.Property(t => t.StartDate).IsRequired();


        builder.Property(t => t.EndDate).IsRequired();


        builder.Property(t => t.IsActive).IsRequired();


        builder.Property(t => t.UsageLimit).IsRequired();


        builder.Property(t => t.UsageLimitPerUser).IsRequired();


        builder.Property(t => t.Deleted);


        builder.Property(t => t.DeletedBy);


        builder.Property(t => t.DeletedByIP).IsUnicode().HasMaxLength(256);


        builder.Property(t => t.IsDelete).IsRequired();


        builder.Property(t => t.LastModified).IsRequired();


        builder.Property(t => t.LastModifiedBy).IsRequired();


        builder.Property(t => t.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.CreatedBy).IsRequired();


        builder.Property(t => t.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();


        builder.Property(t => t.Created).IsRequired();


        builder.Property(t => t.ConvertID).IsUnicode().HasMaxLength(256);


    }
}

