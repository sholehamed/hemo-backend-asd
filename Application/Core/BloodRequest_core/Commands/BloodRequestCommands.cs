using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Core.BloodRequest_core.Commands;
public record CreateNurseRequestCommand(RequestInput Input):IRequest<Guid>;
public record CreateDoctorRequestCommand(RequestInput Input):IRequest<Guid>;
public record UpdateBloodRequestCommand(Guid Guid,byte[] Version,RequestInput Input):IRequest;
public record SubmitBloodRequestCommand(DecisionInput Input):IRequest;
public record CancelBloodRequestCommand(DecisionInput Input):IRequest;
public record DecideBloodRequestCommand(DecisionInput Input):IRequest;

public class CreateNurseRequestValidator:AbstractValidator<CreateNurseRequestCommand>{public CreateNurseRequestValidator(){RuleFor(x=>x.Input).NotNull().SetValidator(new RequestInputValidator());}}
public class CreateDoctorRequestValidator:AbstractValidator<CreateDoctorRequestCommand>{public CreateDoctorRequestValidator(){RuleFor(x=>x.Input).NotNull().SetValidator(new RequestInputValidator());}}
public class UpdateBloodRequestValidator:AbstractValidator<UpdateBloodRequestCommand>{public UpdateBloodRequestValidator(){RuleFor(x=>x.Guid).NotEmpty();RuleFor(x=>x.Version).Must(x=>x?.Length==8);RuleFor(x=>x.Input).NotNull().SetValidator(new RequestInputValidator());}}
public class SubmitBloodRequestValidator:AbstractValidator<SubmitBloodRequestCommand>{public SubmitBloodRequestValidator(){RuleFor(x=>x.Input).NotNull().SetValidator(new DecisionInputValidator());}}
public class CancelBloodRequestValidator:AbstractValidator<CancelBloodRequestCommand>{public CancelBloodRequestValidator(){RuleFor(x=>x.Input).NotNull().SetValidator(new DecisionInputValidator());}}
public class DecideBloodRequestValidator:AbstractValidator<DecideBloodRequestCommand>{public DecideBloodRequestValidator(){RuleFor(x=>x.Input).NotNull().SetValidator(new DecisionInputValidator());}}

public class BloodRequestCommandHandler(BloodRequestService requests,UserRoleService roles,AuthService auth):
    IRequestHandler<CreateNurseRequestCommand,Guid>,IRequestHandler<CreateDoctorRequestCommand,Guid>,
    IRequestHandler<UpdateBloodRequestCommand>,IRequestHandler<SubmitBloodRequestCommand>,IRequestHandler<CancelBloodRequestCommand>,IRequestHandler<DecideBloodRequestCommand>
{
    private IApplicationDbContext Db=>requests.db;
    private static ServiceExeption Error(string message)=>new(message);
    private static void Owner(BloodRequest r,UserRoleInfo user)
    {
        if(r.RequesterUserGUID!=user.UserGuid||r.RequesterRoleGUID!=user.RoleGuid)throw new ForbiddenAccessException();
    }
    private async Task<PatientAdmission> Admission(Guid guid,UserRoleInfo user,CancellationToken ct)=>
        await Db.PatientAdmission.SingleOrDefaultAsync(x=>x.GUID==guid&&x.OrgLocationGUID==user.OrgLocationGUID&&!x.Patient.IsDelete,ct)
        ??throw Error("پذیرش در محدوده مجاز یافت نشد.");
    private async Task<BloodRequest> Load(Guid guid,UserRoleInfo user,CancellationToken ct)=>
        await requests.Table.Include(x=>x.PatientAdmission).Include(x=>x.Items).Include(x=>x.ClinicalData)
            .SingleOrDefaultAsync(x=>x.GUID==guid&&x.PatientAdmission.OrgLocationGUID==user.OrgLocationGUID&&!x.PatientAdmission.Patient.IsDelete,ct)
        ??throw Error("درخواست در محدوده مجاز یافت نشد.");
    private static void CheckVersion(BloodRequest r,byte[] version)
    {if(version==null||!r.Version.SequenceEqual(version))throw Error("درخواست تغییر کرده است؛ دوباره بارگذاری کنید.");}
    private static void Validate<T>(AbstractValidator<T> validator,T data)
    {if(!validator.Validate(data).IsValid)throw Error("اطلاعات درخواست معتبر نیست؛ فیلدهای فرم را بررسی کنید.");}
    private async Task Save(CancellationToken ct)
    {
        try{await requests.SaveChangesAsync(ct);}
        catch(DbUpdateConcurrencyException){throw Error("درخواست همزمان تغییر کرده است؛ دوباره بارگذاری کنید.");}
        catch(DbUpdateException){throw Error("ثبت درخواست انجام نشد؛ اطلاعات را مجدداً بررسی کنید.");}
    }
    private static ClinicalInput Clinical(BloodRequest r)
    {
        var c=r.ClinicalData!;
        return new(c.Diagnosis,c.ClinicalIndication,c.Hemoglobin,c.Hematocrit,c.PlateletCount,c.RecentTransfusion,c.RecentPregnancy,c.PriorReaction,c.UnexpectedAntibody,c.Premedication,c.SampleStatus,c.Notes,
            c.FormDetailsJson==null?null:JsonSerializer.Deserialize<SpecialFormData>(c.FormDetailsJson));
    }
    private void Apply(BloodRequest r,RequestInput input,IEnumerable<BloodRequestItem> existing)
    {
        r.FormType=input.FormType;r.Priority=input.Priority;r.LastModified=DateTime.UtcNow;
        var all=existing.ToList();
        foreach(var line in all.Where(i=>!input.Items.Any(x=>x.ProductCode==i.ProductCode)&&!i.IsDelete))
        {line.IsDelete=true;line.Deleted=DateTime.UtcNow;line.DeletedBy=auth.GetUserGUID();line.DeletedByIP=auth.GetIpAddress();}
        foreach(var inputLine in input.Items)
        {
            var item=all.SingleOrDefault(x=>x.ProductCode==inputLine.ProductCode);
            if(item==null){item=new BloodRequestItem{BloodRequestGUID=r.GUID,ProductCode=inputLine.ProductCode};r.Items.Add(item);Db.BloodRequestItem.Add(item);}
            item.IsDelete=false;item.Deleted=null;item.DeletedBy=null;item.DeletedByIP=null;
            item.Quantity=inputLine.Quantity;item.RequestedBloodGroup=inputLine.RequestedBloodGroup;item.SpecialInstructions=inputLine.SpecialInstructions;
        }
        var c=r.ClinicalData??=new RequestClinicalData{BloodRequestGUID=r.GUID};
        var v=input.Clinical;
        c.Diagnosis=v.Diagnosis;c.ClinicalIndication=v.ClinicalIndication;c.Hemoglobin=v.Hemoglobin;c.Hematocrit=v.Hematocrit;c.PlateletCount=v.PlateletCount;
        c.RecentTransfusion=v.RecentTransfusion;c.RecentPregnancy=v.RecentPregnancy;c.PriorReaction=v.PriorReaction;c.UnexpectedAntibody=v.UnexpectedAntibody;
        c.Premedication=v.Premedication;c.SampleStatus=v.SampleStatus;c.Notes=v.Notes;c.FormDetailsJson=v.FormDetails==null?null:JsonSerializer.Serialize(v.FormDetails);
    }
    private void Log(BloodRequest r,UserRoleInfo user,string action,string before,string? reason,PhysicianCompletion? physician=null)
    {
        Db.RequestApproval.Add(new RequestApproval{BloodRequestGUID=r.GUID,ActorUserGUID=user.UserGuid,ActorRoleGUID=user.RoleGuid,Action=action,PreviousStatus=before,NewStatus=r.Status,
            Reason=reason,SubmissionAttempt=r.SubmissionAttempt,OccurredAt=DateTime.UtcNow,
            SnapshotJson=JsonSerializer.Serialize(new {r.FormType,r.Priority,Items=r.Items.Where(i=>!i.IsDelete).Select(i=>new RequestLineInput(i.ProductCode,i.Quantity,i.RequestedBloodGroup,i.SpecialInstructions)).ToArray(),Clinical=Clinical(r),Physician=physician,AdmissionVersion=r.AdmissionVersionAtSubmission})});
    }
    private async Task<Guid> Create(RequestInput input,string source,string endpoint,CancellationToken ct)
    {
        var user=await BloodRequestAccess.Require(roles,auth,"post",endpoint,true,ct);
        Validate(new RequestInputValidator(),input);
        if(!BloodRequestWorkflow.SourceAllowsForm(source,input.FormType))throw Error("این فرم برای نوع ثبت‌کننده مجاز نیست.");
        var admission=await Admission(input.PatientAdmissionGuid,user,ct);
        var existing=await requests.Table.AsNoTracking().SingleOrDefaultAsync(x=>x.ClientRequestId==input.ClientRequestId,ct);
        if(existing!=null)
        {
            Owner(existing,user);
            if(existing.PatientAdmissionGUID!=admission.GUID||existing.Source!=source)throw Error("شناسه ثبت قبلاً استفاده شده است.");
            return existing.GUID;
        }
        if(admission.Status!="Active")throw Error("پذیرش فعال نیست.");
        var r=new BloodRequest{PatientAdmissionGUID=admission.GUID,ClientRequestId=input.ClientRequestId,RequestNumber="BR-"+Guid.NewGuid().ToString("N").ToUpperInvariant(),RequesterUserGUID=user.UserGuid,RequesterRoleGUID=user.RoleGuid,Source=source,Status=BloodRequestWorkflow.Draft};
        Apply(r,input,Array.Empty<BloodRequestItem>());requests.Table.Add(r);
        await Save(ct);return r.GUID;
    }
    public Task<Guid> Handle(CreateNurseRequestCommand r,CancellationToken ct)=>Create(r.Input,"Nurse","/Nurse",ct);
    public Task<Guid> Handle(CreateDoctorRequestCommand r,CancellationToken ct)=>Create(r.Input,"Doctor","/Doctor",ct);
    public async Task Handle(UpdateBloodRequestCommand command,CancellationToken ct)
    {
        var user=await BloodRequestAccess.Require(roles,auth,"put","",true,ct);
        Validate(new RequestInputValidator(),command.Input);
        var r=await Load(command.Guid,user,ct);Owner(r,user);CheckVersion(r,command.Version);
        if(!BloodRequestWorkflow.CanEdit(r.Status)||r.PatientAdmission.Status!="Active"||r.PatientAdmissionGUID!=command.Input.PatientAdmissionGuid||r.ClientRequestId!=command.Input.ClientRequestId)
            throw Error("این درخواست قابل ویرایش نیست.");
        if(!BloodRequestWorkflow.SourceAllowsForm(r.Source,command.Input.FormType))throw Error("این فرم برای نوع ثبت‌کننده مجاز نیست.");
        var existing=await Db.BloodRequestItem.IgnoreQueryFilters().Where(x=>x.BloodRequestGUID==r.GUID).ToListAsync(ct);
        Apply(r,command.Input,existing);await Save(ct);
    }
    private static void ReadyForSubmission(BloodRequest r)
    {
        if(r.PatientAdmission.Status!="Active"||r.PatientAdmission.IdentityStatus!="Verified")
            throw Error("پذیرش فعال و تطبیق هویت ثبت‌شده لازم است.");
        if(!BloodRequestWorkflow.HasValidItems(r.Items)||r.ClinicalData==null||string.IsNullOrWhiteSpace(r.ClinicalData.ClinicalIndication))
            throw Error("اقلام و علت بالینی درخواست باید تکمیل شود.");
        if(!BloodRequestWorkflow.FormMatchesItems(r.FormType,r.Items))throw Error("اقلام با نوع فرم اختصاصی مطابقت ندارد.");
        var details=Clinical(r).FormDetails;
        if(r.FormType is "WashedRBC" or "Irradiated" or "Apheresis" or "Emergency" or "Autologous")
            if(string.IsNullOrWhiteSpace(details?.SpecialReason))throw Error("علت اختصاصی فرم را ثبت کنید.");
        if(r.FormType=="Emergency"&&r.Priority!="Emergency")throw Error("اولویت فرم اورژانسی باید اورژانس باشد.");
        if(r.FormType=="InterHospital"&&(string.IsNullOrWhiteSpace(details?.OriginHospital)||string.IsNullOrWhiteSpace(details?.DestinationHospital)))
            throw Error("مبدأ و مقصد درخواست انتقال را ثبت کنید.");
        if(r.FormType=="Autologous"&&(details?.ConsentDocumented!=true||details.SurgeryAt==null||string.IsNullOrWhiteSpace(details.Surgery)))
            throw Error("اطلاعات عمل و ثبت مستند رضایت اتولوگ لازم است.");
    }
    private async Task Transition(DecisionInput input,string suffix,bool owner,CancellationToken ct)
    {
        var user=await BloodRequestAccess.Require(roles,auth,"post",suffix,true,ct);
        Validate(new DecisionInputValidator(),input);
        // Keep the admission and request reads stable through the write. Otherwise a
        // concurrent admission correction could slip between the version check and approval.
        await using var transaction=await ((DbContext)Db).Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
        var r=await Load(input.Guid,user,ct);if(owner)Owner(r,user);CheckVersion(r,input.Version);
        var before=r.Status;
        var next=BloodRequestWorkflow.Next(before,input.Action)??throw Error("عملیات در وضعیت فعلی مجاز نیست.");
        if(input.Action=="Submit")
        {
            ReadyForSubmission(r);r.AdmissionVersionAtSubmission=r.PatientAdmission.Version.ToArray();r.SubmissionAttempt++;r.SubmittedAt=DateTime.UtcNow;
        }
        if(input.Action=="Approve")
        {
            ReadyForSubmission(r);
            if(r.AdmissionVersionAtSubmission==null||!r.AdmissionVersionAtSubmission.SequenceEqual(r.PatientAdmission.Version))
                throw Error("اطلاعات پذیرش بعد از ارسال تغییر کرده؛ درخواست را برای بازبینی برگردانید.");
            if(input.Physician?.Confirmed!=true)throw Error("تأیید صریح پزشک لازم است.");
        }
        r.Status=next;r.LastModified=DateTime.UtcNow;
        Log(r,user,input.Action,before,input.Reason,input.Action=="Approve"?input.Physician:null);
        await Save(ct);
        await transaction.CommitAsync(ct);
    }
    public Task Handle(SubmitBloodRequestCommand r,CancellationToken ct)=>r.Input.Action=="Submit"?Transition(r.Input,"/Submit",true,ct):throw Error("عملیات نامعتبر است.");
    public Task Handle(CancelBloodRequestCommand r,CancellationToken ct)=>r.Input.Action=="Cancel"?Transition(r.Input,"/Cancel",true,ct):throw Error("عملیات نامعتبر است.");
    public Task Handle(DecideBloodRequestCommand r,CancellationToken ct)=>r.Input.Action is "Approve" or "Return" or "Reject"?Transition(r.Input,"/Decide",false,ct):throw Error("عملیات نامعتبر است.");
}
