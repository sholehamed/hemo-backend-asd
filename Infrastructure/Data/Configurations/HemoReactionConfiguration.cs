using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Data.Configurations;
public class HemoReactionConfiguration : IEntityTypeConfiguration<HemoReaction>
{
    public void Configure(EntityTypeBuilder<HemoReaction> builder)
    {
        builder.ToTable("HemoReaction");builder.HasKey(x=>x.GUID);builder.HasQueryFilter(x=>!x.IsDelete);
        builder.Property(x=>x.ConvertID).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CreatedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.LastModifiedByIP).IsUnicode().HasMaxLength(256).IsRequired();
        builder.Property(x=>x.DeletedByIP).IsUnicode().HasMaxLength(256);
        builder.Property(x=>x.CaseNumber).IsUnicode().HasMaxLength(80).IsRequired();
        builder.Property(x=>x.Status).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.SymptomsJson).IsUnicode().IsRequired();
        builder.Property(x=>x.InitialSeverity).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.InitialNotes).IsUnicode().HasMaxLength(4000).IsRequired();
        builder.Property(x=>x.ActionsTaken).IsUnicode().HasMaxLength(4000).IsRequired();
        builder.Property(x=>x.SamplesSent).IsUnicode().HasMaxLength(2000);
        builder.Property(x=>x.VolumeMl).HasPrecision(9,2);
        builder.Property(x=>x.DiagnosisCode).IsUnicode().HasMaxLength(64);
        builder.Property(x=>x.Severity).IsUnicode().HasMaxLength(32);
        builder.Property(x=>x.Imputability).IsUnicode().HasMaxLength(32);
        builder.Property(x=>x.Outcome).IsUnicode().HasMaxLength(64);
        builder.Property(x=>x.DoctorNotes).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.DoctorActions).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.DoctorLabRequest).IsUnicode().HasMaxLength(2000);
        builder.Property(x=>x.FutureRecommendations).IsUnicode().HasMaxLength(2000);
        builder.Property(x=>x.DoctorReference).IsUnicode().HasMaxLength(100);
        builder.Property(x=>x.BankFindings).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.BankSampleReference).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.BankReportReference).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.BankDisposition).IsUnicode().HasMaxLength(2000);
        builder.Property(x=>x.InvestigationNotes).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.ExternalStatus).IsUnicode().HasMaxLength(32).IsRequired();
        builder.Property(x=>x.ExternalReference).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.RcaLead).IsUnicode().HasMaxLength(200);
        builder.Property(x=>x.RcaMethod).IsUnicode().HasMaxLength(64);
        builder.Property(x=>x.RcaParticipants).IsUnicode().HasMaxLength(2000);
        builder.Property(x=>x.DirectCause).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.RootCause).IsUnicode().HasMaxLength(4000);
        builder.Property(x=>x.Version).IsRowVersion();
        builder.HasIndex(x=>x.ClientReportId).IsUnique();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.CaseNumber }).IsUnique();
        builder.HasIndex(x=>new { x.OrgLocationGUID, x.Status, x.OnsetAt });
        builder.HasIndex(x=>new { x.BloodTransferGUID, x.Status });
        builder.HasOne(x=>x.OrgLocation).WithMany().HasForeignKey(x=>x.OrgLocationGUID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x=>x.Transfer).WithMany().HasForeignKey(x=>x.BloodTransferGUID).OnDelete(DeleteBehavior.Restrict);
    }
}
