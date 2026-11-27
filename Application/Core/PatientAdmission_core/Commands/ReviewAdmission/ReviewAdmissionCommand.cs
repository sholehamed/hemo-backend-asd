using Application.Common.Exceptions;
using Application.Common.Utility.Security;

namespace Application.Core.PatientAdmission_core.Commands.ReviewAdmission;

public record ReviewAdmissionCommand(Guid Guid,string Kind,string Outcome,int Checklist,string? Notes,byte[] Version) : IRequest;
public class ReviewAdmissionValidator : AbstractValidator<ReviewAdmissionCommand>
{
    public ReviewAdmissionValidator()
    {
        RuleFor(x=>x.Guid).NotEmpty();RuleFor(x=>x.Version).NotEmpty();RuleFor(x=>x.Checklist).InclusiveBetween(0,31);RuleFor(x=>x.Notes).MaximumLength(2000);
        RuleFor(x=>x).Must(x=>(x.Kind=="Identity"&&(x.Outcome=="Mismatch"||(x.Outcome=="Verified"&&AdmissionRules.CompleteChecklist(x.Checklist))))||(x.Kind=="History"&&x.Outcome=="Reviewed")||(x.Kind=="Clearance"&&(x.Outcome=="Draft"||(x.Outcome=="Ready"&&AdmissionRules.CompleteChecklist(x.Checklist))))).WithMessage("نوع بررسی یا چک‌لیست نامعتبر است.");
        RuleFor(x=>x.Notes).NotEmpty().When(x=>x.Outcome=="Mismatch"||x.Kind=="History");
    }
}
public class ReviewAdmissionCommandHandler(PatientAdmissionService admissions,PatientAdmissionReviewService reviews,UserRoleService roles,AuthService auth) : IRequestHandler<ReviewAdmissionCommand>
{
    public async Task Handle(ReviewAdmissionCommand r,CancellationToken ct)
    {
        var scope=await AdmissionScope.GetAsync(roles,auth,ct);
        var a=await admissions.Table.SingleOrDefaultAsync(x=>x.GUID==r.Guid&&x.OrgLocationGUID==scope.OrgLocationGUID,ct)??throw new ServiceExeption("پرونده یافت نشد.");
        if(!a.Version.SequenceEqual(r.Version))throw new ServiceExeption("پرونده تغییر کرده است؛ دوباره بارگذاری کنید.");
        var now=DateTime.UtcNow;
        switch(r.Kind)
        {
            case "Identity":a.IdentityStatus=r.Outcome;a.IdentityReviewedAt=now;if(r.Outcome=="Mismatch")a.Status="Active";break;
            case "History":a.HistoryReviewedAt=now;break;
            case "Clearance":
                if(r.Outcome=="Ready"&&(a.IdentityStatus!="Verified"||!a.HistoryReviewedAt.HasValue))throw new ServiceExeption("تطبیق هویت و بررسی سوابق باید ابتدا ثبت شود.");
                // A documented readiness review, not an actual HIS discharge or automated clinical authorization.
                a.ClearanceChecklist=r.Checklist;a.ClearanceNotes=r.Notes;a.ClearanceReviewedAt=now;a.Status=r.Outcome=="Ready"?"ReadyForDischarge":"Active";break;
            default:throw new ServiceExeption("عملیات نامعتبر است.");
        }
        reviews.Table.Add(new PatientAdmissionReview{PatientAdmissionGUID=a.GUID,Kind=r.Kind,Outcome=r.Outcome,Checklist=r.Checklist,Notes=r.Notes,ReviewedAt=now});
        try{await admissions.SaveChangesAsync(ct);}catch(DbUpdateConcurrencyException){throw new ServiceExeption("پرونده همزمان تغییر کرده است؛ دوباره بارگذاری کنید.");}
    }
}
