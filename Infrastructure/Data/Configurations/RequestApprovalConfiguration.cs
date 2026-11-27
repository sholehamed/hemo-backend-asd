using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class RequestApprovalConfiguration : IEntityTypeConfiguration<RequestApproval>
{
    public void Configure(EntityTypeBuilder<RequestApproval> builder)
    {
        builder.ToTable("RequestApproval");
        builder.HasKey(x=>x.GUID);
        builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.Action).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.PreviousStatus).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.NewStatus).IsUnicode().HasMaxLength(24).IsRequired();
        builder.Property(x=>x.Reason).IsUnicode().HasMaxLength(2000);
        builder.HasIndex(x=>new { x.BloodRequestGUID, x.OccurredAt });
        builder.HasOne(x=>x.BloodRequest).WithMany(x=>x.Approvals).HasForeignKey(x=>x.BloodRequestGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
