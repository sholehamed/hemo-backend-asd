using Application.Common.Exceptions;
using Application.Common.Utility.Security;

namespace Application.Core.PatientAdmission_core.Commands.UpdateAdmission;

public record UpdateAdmissionCommand(Guid Guid,string Ward,string BedNumber,string Physician,string? Diagnosis,string? Insurance,string? Notes,byte[] Version) : IRequest;
public class UpdateAdmissionValidator : AbstractValidator<UpdateAdmissionCommand>
{
    public UpdateAdmissionValidator()
    {
        RuleFor(x=>x.Guid).NotEmpty();RuleFor(x=>x.Version).NotEmpty();RuleFor(x=>x.Ward).NotEmpty().MaximumLength(100);RuleFor(x=>x.BedNumber).NotEmpty().MaximumLength(64);RuleFor(x=>x.Physician).NotEmpty().MaximumLength(100);RuleFor(x=>x.Diagnosis).MaximumLength(500);RuleFor(x=>x.Insurance).MaximumLength(100);RuleFor(x=>x.Notes).MaximumLength(2000);
    }
}
public class UpdateAdmissionCommandHandler(PatientAdmissionService admissions,UserRoleService roles,AuthService auth) : IRequestHandler<UpdateAdmissionCommand>
{
    public async Task Handle(UpdateAdmissionCommand r,CancellationToken ct)
    {
        var scope=await AdmissionScope.GetAsync(roles,auth,"put","/Identity/PatientAdmission",ct);
        var a=await admissions.Table.SingleOrDefaultAsync(x=>x.GUID==r.Guid&&x.OrgLocationGUID==scope.OrgLocationGUID,ct)??throw new ServiceExeption("پرونده یافت نشد.");
        if(!a.Version.SequenceEqual(r.Version))throw new ServiceExeption("پرونده تغییر کرده است؛ دوباره بارگذاری کنید.");
        a.Ward=r.Ward.Trim();a.BedNumber=r.BedNumber.Trim();a.Physician=r.Physician.Trim();a.Diagnosis=r.Diagnosis;a.Insurance=r.Insurance;a.Notes=r.Notes;
        a.Status="Active";a.ClearanceChecklist=0;a.ClearanceReviewedAt=null;a.ClearanceNotes=null;
        try{await admissions.SaveChangesAsync(ct);}catch(DbUpdateConcurrencyException){throw new ServiceExeption("پرونده همزمان تغییر کرده است.");}
    }
}
