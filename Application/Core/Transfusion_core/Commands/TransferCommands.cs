using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using Application.Core.BloodBank_core;
using System.Text.Json;
namespace Application.Core.Transfusion_core.Commands;
public record DispatchTransferCommand(DispatchInput Input):IRequest<Guid>;
public record TransferActionCommand(TransferActionInput Input,string Action):IRequest;
public record RecordTransferVitalCommand(VitalInput Input):IRequest<Guid>;
public class TransferCommandHandler(BloodTransferService transfers,UserRoleService roles,AuthService auth):
    IRequestHandler<DispatchTransferCommand,Guid>,IRequestHandler<TransferActionCommand>,IRequestHandler<RecordTransferVitalCommand,Guid>
{
    private IApplicationDbContext Db=>transfers.db;
    private static ServiceExeption Error(string message)=>new(message);
    private static void Validate<T>(AbstractValidator<T> validator,T value){if(value==null||!validator.Validate(value).IsValid)throw Error("اطلاعات عملیات معتبر نیست؛ فرم را بررسی کنید.");}
    private static void Check(byte[] actual,byte[] version){if(version==null||!actual.SequenceEqual(version))throw Error("نسخه اطلاعات تغییر کرده است؛ دوباره دریافت کنید.");}
    private static bool DatabaseFailure(Exception? e){for(var x=e;x!=null;x=x.InnerException)if(x is System.Data.Common.DbException or DbUpdateException)return true;return false;}
    private async Task<T> Atomic<T>(Func<Task<T>> action,CancellationToken ct)
    {
        try{
            await using var tx=await ((DbContext)Db).Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
            var result=await action();await transfers.SaveChangesAsync(ct);await tx.CommitAsync(ct);return result;
        }catch(Exception e) when(DatabaseFailure(e)){throw Error("ثبت همزمان یا خطای دیتابیس؛ اطلاعات را دوباره دریافت و بررسی کنید.");}
    }
    private async Task<BloodTransfer> Load(Guid id,UserRoleInfo actor,CancellationToken ct)=>
        await transfers.Table.Include(t=>t.Unit).Include(t=>t.Admission).ThenInclude(a=>a.Patient).Include(t=>t.CompatibilityTest)
            .SingleOrDefaultAsync(t=>t.GUID==id&&t.OrgLocationGUID==actor.OrgLocationGUID&&t.Unit.OrgLocationGUID==actor.OrgLocationGUID&&t.Admission.OrgLocationGUID==actor.OrgLocationGUID,ct)
            ??throw Error("حواله در محدوده مجاز یافت نشد.");
    private async Task<BloodRequest> Request(Guid guid,UserRoleInfo actor,CancellationToken ct)=>
        await Db.BloodRequest.Include(r=>r.PatientAdmission).ThenInclude(a=>a.Patient).Include(r=>r.Items).Include(r=>r.Approvals).AsSingleQuery()
            .SingleOrDefaultAsync(r=>r.GUID==guid&&r.PatientAdmission.OrgLocationGUID==actor.OrgLocationGUID&&!r.PatientAdmission.Patient.IsDelete,ct)
            ??throw Error("درخواست معتبر در مرکز یافت نشد.");
    private async Task RequireCurrent(BloodTransfer t,UserRoleInfo actor,CancellationToken ct)
    {
        var r=await Request(t.BloodRequestGUID,actor,ct);
        if(t.Unit.Status!="Issued"||!TransferRules.Current(t,r,t.Unit,t.CompatibilityTest,DateTime.UtcNow))throw Error("تأیید، انقضا، هویت یا وضعیت واحد تغییر کرده است؛ ادامه عملیات مجاز نیست.");
    }
    private static void Codes(TransferActionInput v,BloodTransfer t)
    {
        if(!TransferRules.ExactCodes(v.UnitCode,v.WristbandCode,t)||v.Checks!=TransferRules.RequiredChecks)
            throw Error("شناسه‌های مشاهده‌شده و چک‌لیست ثبت‌شده باید منطبق باشند.");
    }
    private static void UnitCode(TransferActionInput v,BloodTransfer t)
    {
        if(!string.Equals(v.UnitCode?.Trim(),t.UnitCode,StringComparison.Ordinal))throw Error("کد واحد مشاهده‌شده مطابقت ندارد.");
    }
    private void Log(BloodTransfer t,UserRoleInfo a,string action,string before,string? notes,object? evidence=null)
    {
        Db.BloodTransferEvent.Add(new BloodTransferEvent{BloodTransferGUID=t.GUID,ActorUserGUID=a.UserGuid,ActorRoleGUID=a.RoleGuid,Action=action,PreviousStatus=before,NewStatus=t.Status,OccurredAt=DateTime.UtcNow,Notes=notes,EvidenceJson=evidence==null?null:JsonSerializer.Serialize(evidence)});
    }
    private void ChangeUnit(BankBloodUnit u,BloodTransfer t,UserRoleInfo a,string status,string action,string? notes)
    {
        var before=u.Status;u.Status=status;u.LastModified=DateTime.UtcNow;
        Db.BankUnitEvent.Add(new BankUnitEvent{BankBloodUnitGUID=u.GUID,BloodRequestGUID=t.BloodRequestGUID,ActorUserGUID=a.UserGuid,ActorRoleGUID=a.RoleGuid,Action=action,PreviousStatus=before,NewStatus=status,OccurredAt=DateTime.UtcNow,Reason=notes});
    }
    public async Task<Guid> Handle(DispatchTransferCommand cmd,CancellationToken ct)
    {
        var actor=await TransferAccess.Require(roles,auth,"post","/Dispatch",ct);var v=cmd.Input;Validate(new DispatchInputValidator(),v);
        return await Atomic(async()=>{
            var u=await Db.BankBloodUnit.SingleOrDefaultAsync(x=>x.GUID==v.UnitGuid&&x.OrgLocationGUID==actor.OrgLocationGUID,ct)??throw Error("واحد در مرکز یافت نشد.");
            Check(u.Version,v.UnitVersion);var now=DateTime.UtcNow;
            if(u.Status!=BankRules.Ready||!u.BloodRequestGUID.HasValue||BankRules.IsExpired(u.ExpiresAt,now)||
                BankRules.NeedsThaw(u.ProductCode)&&!u.ThawCompletedAt.HasValue||await transfers.Table.IgnoreQueryFilters().AnyAsync(t=>t.BankBloodUnitGUID==u.GUID,ct))
                throw Error("واحد آماده صدور نیست یا قبلاً حواله دارد.");
            var r=await Request(u.BloodRequestGUID.Value,actor,ct);var a=r.PatientAdmission;
            if(!BankRules.CurrentApproval(r)||!r.Items.Any(i=>!i.IsDelete&&i.ProductCode==u.ProductCode))throw Error("تأیید پزشک یا پذیرش نیازمند بازبینی است.");
            var lab=await Db.BankCompatibilityTest.Where(x=>x.BankBloodUnitGUID==u.GUID&&x.BloodRequestGUID==r.GUID)
                .OrderByDescending(x=>x.RecordedAt).ThenByDescending(x=>x.GUID).FirstOrDefaultAsync(ct);
            var temp=await Db.BankTemperatureLog.Where(x=>x.OrgLocationGUID==actor.OrgLocationGUID&&x.EquipmentCode==u.EquipmentCode)
                .OrderByDescending(x=>x.MeasuredAt).ThenByDescending(x=>x.RecordedAt).ThenByDescending(x=>x.GUID).FirstOrDefaultAsync(ct);
            if(lab==null||!BankRules.LabCurrent(lab,r,u)||temp?.Assessment!="Acceptable")throw Error("نتیجه معتبر آزمایش و ارزیابی دمای ثبت‌شده لازم است.");
            var t=new BloodTransfer{OrgLocationGUID=actor.OrgLocationGUID,BankBloodUnitGUID=u.GUID,BloodRequestGUID=r.GUID,PatientAdmissionGUID=a.GUID,BankCompatibilityTestGUID=lab.GUID,
                TransferNumber="TX-"+Guid.NewGuid().ToString("N").ToUpperInvariant(),PatientName=a.Patient.FirstName+" "+a.Patient.LastName,HisNumber=a.HISNumber,Ward=a.Ward,WristbandCode=a.WristbandCode,UnitCode=u.UnitCode,ProductCode=u.ProductCode,BloodGroup=u.BloodGroup,
                RequestVersion=r.Version.ToArray(),AdmissionVersion=a.Version.ToArray(),Status=TransferRules.Dispatched,DispatchedAt=now,DispatchedBy=actor.UserGuid,CarrierName=v.CarrierName.Trim(),CarrierCode=v.CarrierCode.Trim(),ContainerCode=v.ContainerCode.Trim(),DispatchTemperature=v.Temperature,TransportIndicator=v.TransportIndicator.Trim(),PackCount=v.PackCount};
            if(!TransferRules.ExactCodes(v.UnitCode,v.WristbandCode,t))throw Error("شناسه واحد یا مچ‌بند ثبت‌شده مطابقت ندارد.");
            transfers.Table.Add(t);ChangeUnit(u,t,actor,"Issued","Dispatch",v.Notes);
            Log(t,actor,"Dispatch","",v.Notes,new{v.Checks,v.UnitCode,v.WristbandCode,v.CarrierName,v.CarrierCode,v.ContainerCode,v.Temperature,v.TransportIndicator,v.PackCount});
            return t.GUID;
        },ct);
    }
    public async Task Handle(TransferActionCommand cmd,CancellationToken ct)
    {
        var action=cmd.Action;
        if(!new[]{"Receive","ReceiptHold","Verify","Witness","Start","Complete","Stop","Return","AcceptReturn","Dispose","DoctorNote"}.Contains(action))throw Error("عملیات نامعتبر است.");
        var actor=await TransferAccess.Require(roles,auth,"post",action=="ReceiptHold"?"/Receive":"/"+action,ct);var v=cmd.Input;Validate(new TransferActionValidator(),v);
        await Atomic(async()=>{
            var t=await Load(v.Guid,actor,ct);Check(t.Version,v.Version);var before=t.Status;var now=DateTime.UtcNow;
            var next=action=="DoctorNote"?before:TransferRules.Next(before,action)??throw Error("عملیات در وضعیت فعلی مجاز نیست.");
            if(action is "Receive" or "Verify" or "Witness" or "Start")await RequireCurrent(t,actor,ct);
            if(action is "Receive" or "Verify" or "Witness" or "Start")Codes(v,t);
            if(action is "ReceiptHold" or "Return" or "AcceptReturn" or "Dispose")UnitCode(v,t);
            switch(action)
            {
                case "Receive":
                    if(v.Temperature==null||v.Condition!="Acceptable"||string.IsNullOrWhiteSpace(v.ReceiverName))throw Error("شرایط و نام تحویل‌گیرنده را ثبت کنید.");
                    t.ReceivedAt=now;t.ReceivedBy=actor.UserGuid;break;
                case "ReceiptHold":
                    if(string.IsNullOrWhiteSpace(v.ReceiverName)||string.IsNullOrWhiteSpace(v.Condition)||v.Condition=="Acceptable")throw Error("نام تحویل‌گیرنده و دلیل مغایرت را ثبت کنید.");
                    t.ReceivedAt=now;t.ReceivedBy=actor.UserGuid;break;
                case "Verify":
                    t.VerifiedAt=now;t.VerifiedBy=actor.UserGuid;t.WitnessedAt=null;t.WitnessedBy=null;break;
                case "Witness":
                    if(!TransferRules.DistinctWitness(t.VerifiedBy,actor.UserGuid))throw Error("شاهد باید با حساب مستقل وارد شود.");
                    t.WitnessedAt=now;t.WitnessedBy=actor.UserGuid;break;
                case "Start":
                    if(t.VerifiedBy!=actor.UserGuid||!TransferRules.DistinctWitness(t.VerifiedBy,t.WitnessedBy??Guid.Empty)||!t.VerifiedAt.HasValue||
                        !await Db.BloodTransferVital.AnyAsync(x=>x.BloodTransferGUID==t.GUID&&x.Phase=="T0"&&x.MeasuredAt>=t.VerifiedAt.Value,ct)||
                        string.IsNullOrWhiteSpace(v.NeedleReference))throw Error("تطبیق دو حساب، پایش اولیه بعد از تطبیق و ثبت مشخصات وسیله لازم است.");
                    t.StartedAt=now;t.StartedBy=actor.UserGuid;break;
                case "Complete":
                    if(v.SuspectedReaction||t.SuspectedReaction||!t.StartedAt.HasValue||!await Db.BloodTransferVital.AnyAsync(x=>x.BloodTransferGUID==t.GUID&&x.Phase=="End"&&x.MeasuredAt>=t.StartedAt.Value,ct))throw Error("پایش پایان و نبود گزارش نگرانی ثبت‌شده لازم است؛ در صورت نگرانی توقف را ثبت کنید.");
                    t.EndedAt=now;t.EndedBy=actor.UserGuid;ChangeUnit(t.Unit,t,actor,"Transfused","Complete",v.Notes);break;
                case "Stop":
                    t.EndedAt=now;t.EndedBy=actor.UserGuid;t.SuspectedReaction|=v.SuspectedReaction;ChangeUnit(t.Unit,t,actor,"TransfusionStopped","Stop",v.Notes);break;
                case "Return":
                    if(string.IsNullOrWhiteSpace(v.Condition)||string.IsNullOrWhiteSpace(v.ReceiverName))throw Error("وضعیت واحد و فرد انتقال‌دهنده لازم است.");
                    t.ReturnedAt=now;ChangeUnit(t.Unit,t,actor,"Returning","Return",v.Notes);break;
                case "AcceptReturn":
                    if(v.Temperature==null||string.IsNullOrWhiteSpace(v.Condition)||string.IsNullOrWhiteSpace(v.ReceiverName))throw Error("وضعیت، دما و نام تحویل‌گیرنده ثبت شود.");
                    t.ReturnAcceptedAt=now;ChangeUnit(t.Unit,t,actor,"ReturnedHold","AcceptReturn",v.Notes);break;
                case "Dispose":
                    ChangeUnit(t.Unit,t,actor,"ReturnedDisposed","Dispose",v.Notes);break;
                case "DoctorNote":
                    // A note is not a restart or treatment order. Concern is retained for ST-06.
                    t.SuspectedReaction|=v.SuspectedReaction;break;
            }
            t.Status=next;t.LastModified=now;
            Log(t,actor,action,before,v.Notes,new{v.UnitCode,v.WristbandCode,v.Checks,v.Temperature,v.Condition,v.ReceiverName,v.NeedleReference,v.SuspectedReaction});
            return true;
        },ct);
    }
    public async Task<Guid> Handle(RecordTransferVitalCommand cmd,CancellationToken ct)
    {
        var actor=await TransferAccess.Require(roles,auth,"post","/Vital",ct);var v=cmd.Input;Validate(new VitalInputValidator(),v);
        return await Atomic(async()=>{
            var t=await Load(v.Guid,actor,ct);Check(t.Version,v.Version);var now=DateTime.UtcNow;var measured=v.MeasuredAt.UtcDateTime;
            if(!TransferRules.VitalAllowed(t.Status,v.Phase)||measured>now||measured<t.DispatchedAt)throw Error("زمان یا نوبت پایش برای وضعیت جاری معتبر نیست.");
            if(v.Phase=="T0"&&(!t.VerifiedAt.HasValue||measured<t.VerifiedAt.Value)||
                (v.Phase is "T15" or "Mid" or "End")&&(!t.StartedAt.HasValue||measured<t.StartedAt.Value)||
                v.Phase=="Post1h"&&(!t.EndedAt.HasValue||measured<t.EndedAt.Value))throw Error("زمان اندازه‌گیری با مرحله حواله سازگار نیست.");
            var row=new BloodTransferVital{BloodTransferGUID=t.GUID,ActorUserGUID=actor.UserGuid,ActorRoleGUID=actor.RoleGuid,Phase=v.Phase,MeasuredAt=measured,RecordedAt=now,Systolic=v.Systolic,Diastolic=v.Diastolic,Pulse=v.Pulse,Temperature=v.Temperature,Spo2=v.Spo2,Respiration=v.Respiration,UrineMl=v.UrineMl,Notes=v.Notes};
            Db.BloodTransferVital.Add(row);t.LastModified=now;Log(t,actor,"Vital",t.Status,v.Notes,new{VitalGuid=row.GUID,v.Phase,MeasuredAt=measured});return row.GUID;
        },ct);
    }
}
