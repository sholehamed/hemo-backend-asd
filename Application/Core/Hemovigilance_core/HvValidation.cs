namespace Application.Core.Hemovigilance_core;
public class ReactionInputValidator:AbstractValidator<ReactionInput>
{
    public ReactionInputValidator(){
        RuleFor(x=>x.TransferGuid).NotEmpty();RuleFor(x=>x.TransferVersion).Must(x=>x?.Length==8);RuleFor(x=>x.ClientReportId).NotEmpty();RuleFor(x=>x.OnsetAt).NotEmpty();
        RuleFor(x=>x.Symptoms).Must(x=>x!=null&&x.Length>0&&x.Length<=HvRules.Symptoms.Length&&x.Distinct().Count()==x.Length&&x.All(HvRules.Symptoms.Contains)&&(!x.Contains("Unknown")||x.Length==1));
        RuleFor(x=>x.InitialSeverity).Must(x=>HvRules.Severities.Contains(x));RuleFor(x=>x.Notes).NotEmpty().MaximumLength(4000);RuleFor(x=>x.ActionsTaken).NotEmpty().MaximumLength(4000);
        RuleFor(x=>x.SamplesSent).MaximumLength(2000);RuleFor(x=>x.VolumeMl).Must(x=>x==null||x>=0&&x<=1000000);
    }
}
public class ReviewInputValidator:AbstractValidator<ReviewInput>
{
    public ReviewInputValidator(){
        RuleFor(x=>x.Guid).NotEmpty();RuleFor(x=>x.Version).Must(x=>x?.Length==8);RuleFor(x=>x.Notes).NotEmpty().MaximumLength(4000);
        RuleFor(x=>x.DiagnosisCode).MaximumLength(64);RuleFor(x=>x.Severity).MaximumLength(32);RuleFor(x=>x.Imputability).MaximumLength(32);RuleFor(x=>x.Outcome).MaximumLength(64);
        RuleFor(x=>x.Actions).MaximumLength(4000);RuleFor(x=>x.LabRequest).MaximumLength(2000);RuleFor(x=>x.Recommendations).MaximumLength(2000);RuleFor(x=>x.DoctorReference).MaximumLength(100);
        RuleFor(x=>x.SampleReference).MaximumLength(200);RuleFor(x=>x.ReportReference).MaximumLength(200);RuleFor(x=>x.Disposition).MaximumLength(2000);
        RuleFor(x=>x.ExternalStatus).MaximumLength(32);RuleFor(x=>x.ExternalReference).MaximumLength(200);RuleFor(x=>x.RcaLead).MaximumLength(200);RuleFor(x=>x.RcaMethod).MaximumLength(64);
        RuleFor(x=>x.Participants).MaximumLength(2000);RuleFor(x=>x.DirectCause).MaximumLength(4000);RuleFor(x=>x.RootCause).MaximumLength(4000);
    }
}
public class CapaCreateValidator:AbstractValidator<CapaCreateInput>
{
    public CapaCreateValidator(){RuleFor(x=>x.Guid).NotEmpty();RuleFor(x=>x.Version).Must(x=>x?.Length==8);RuleFor(x=>x.Title).NotEmpty().MaximumLength(300);RuleFor(x=>x.Kind).Must(x=>x=="Corrective"||x=="Preventive");RuleFor(x=>x.OwnerUserGuid).NotEmpty();RuleFor(x=>x.DueAt).NotEmpty();RuleFor(x=>x.Indicator).NotEmpty().MaximumLength(1000);}
}
public class CapaActionValidator:AbstractValidator<CapaActionInput>
{
    public CapaActionValidator(){RuleFor(x=>x.Guid).NotEmpty();RuleFor(x=>x.Version).Must(x=>x?.Length==8);RuleFor(x=>x.CaseVersion).Must(x=>x?.Length==8);RuleFor(x=>x.Progress).InclusiveBetween(0,100);RuleFor(x=>x.Evidence).NotEmpty().MaximumLength(4000);}
}
public class ReportInputValidator:AbstractValidator<ReportInput>
{
    public ReportInputValidator(){RuleFor(x=>x.Title).NotEmpty().MaximumLength(300);RuleFor(x=>x.From).NotEmpty();RuleFor(x=>x.To).Must((v,to)=>to>=v.From&&to-v.From<=TimeSpan.FromDays(3660)&&to<=DateTimeOffset.UtcNow.AddDays(1));RuleFor(x=>x.Ward).MaximumLength(120);RuleFor(x=>x.ProductCode).MaximumLength(32);}
}
