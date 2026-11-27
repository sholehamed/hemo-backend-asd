using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using System.Text.Json;
namespace Application.Core.BloodBank_core.Commands;
public record ReceiveBankUnitCommand(ReceiveUnit Input):IRequest<Guid>;
public record BankUnitActionCommand(UnitAction Input,string Endpoint):IRequest;
public record RecordBankLabCommand(LabInput Input):IRequest<Guid>;
public record RecordBankTemperatureCommand(TemperatureInput Input):IRequest<Guid>;
public record BankReturnRequestCommand(ReturnRequestInput Input):IRequest;

public class BankCommandHandler(BankBloodUnitService units,UserRoleService roles,AuthService auth):
    IRequestHandler<ReceiveBankUnitCommand,Guid>,IRequestHandler<BankUnitActionCommand>,IRequestHandler<RecordBankLabCommand,Guid>,IRequestHandler<RecordBankTemperatureCommand,Guid>,IRequestHandler<BankReturnRequestCommand>
{
    private IApplicationDbContext Db=>units.db;
    private static ServiceExeption Error(string s)=>new(s);
    private static void Validate<T>(AbstractValidator<T> validator,T input)
    {if(input==null||!validator.Validate(input).IsValid)throw Error("اطلاعات عملیات بانک خون معتبر نیست.");}
    private async Task Save(CancellationToken ct)
    {
        try{await units.SaveChangesAsync(ct);}
        catch(DbUpdateConcurrencyException){throw Error("اطلاعات همزمان تغییر کرده است؛ دوباره دریافت کنید.");}
        catch(DbUpdateException){throw Error("ثبت انجام نشد؛ اطلاعات و شناسه واحد را بررسی کنید.");}
    }
    private Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> Transaction(CancellationToken ct)=>
        ((DbContext)Db).Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
    private async Task<BankBloodUnit> Unit(Guid guid,UserRoleInfo user,CancellationToken ct)=>
        await units.Table.SingleOrDefaultAsync(x=>x.GUID==guid&&x.OrgLocationGUID==user.OrgLocationGUID,ct)??throw Error("واحد در محدوده مجاز یافت نشد.");
    private async Task<BloodRequest> Request(Guid guid,UserRoleInfo user,CancellationToken ct)=>
        await Db.BloodRequest.Include(r=>r.PatientAdmission).Include(r=>r.Items).Include(r=>r.Approvals).AsSingleQuery()
            .SingleOrDefaultAsync(r=>r.GUID==guid&&r.PatientAdmission.OrgLocationGUID==user.OrgLocationGUID&&!r.PatientAdmission.Patient.IsDelete,ct)??throw Error("درخواست در محدوده مجاز یافت نشد.");
    private static void Check(byte[] current,byte[]? expected){if(expected==null||!current.SequenceEqual(expected))throw Error("نسخه اطلاعات تغییر کرده است؛ مجدداً دریافت کنید.");}
    private static void Current(BloodRequest r){if(!BankRules.CurrentApproval(r))throw Error("تأیید پزشک یا پذیرش دیگر معتبر نیست؛ درخواست باید بازبینی شود.");}
    private static void Live(BankBloodUnit u){if(BankRules.IsExpired(u.ExpiresAt,DateTime.UtcNow))throw Error("تاریخ انقضای ثبت‌شده واحد گذشته است.");}
    private async Task TemperatureOk(BankBloodUnit u,CancellationToken ct)
    {
        var last=await Db.BankTemperatureLog.AsNoTracking().Where(t=>t.OrgLocationGUID==u.OrgLocationGUID&&t.EquipmentCode==u.EquipmentCode)
            .OrderByDescending(t=>t.MeasuredAt).ThenByDescending(t=>t.RecordedAt).ThenByDescending(t=>t.GUID).FirstOrDefaultAsync(ct);
        if(last?.Assessment!="Acceptable")throw Error("ثبت و بررسی وضعیت دمای تجهیز لازم است.");
    }
    private async Task LabOk(BankBloodUnit u,BloodRequest r,CancellationToken ct)
    {
        var last=await Db.BankCompatibilityTest.AsNoTracking().Where(t=>t.BankBloodUnitGUID==u.GUID&&t.BloodRequestGUID==r.GUID)
          .OrderByDescending(t=>t.RecordedAt).ThenByDescending(t=>t.GUID).FirstOrDefaultAsync(ct);
        if(last==null||!BankRules.LabCurrent(last,r,u))throw Error("نتیجه معتبر و مستند آزمایش برای همین واحد و درخواست لازم است.");
    }
    private void Event(BankBloodUnit u,UserRoleInfo user,string action,string before,Guid? request,string? reason,string? method=null,decimal? temp=null,DateTime? expiry=null)
    {
        Db.BankUnitEvent.Add(new BankUnitEvent{BankBloodUnitGUID=u.GUID,BloodRequestGUID=request,ActorUserGUID=user.UserGuid,ActorRoleGUID=user.RoleGuid,Action=action,PreviousStatus=before,NewStatus=u.Status,OccurredAt=DateTime.UtcNow,Reason=reason,Method=method,Temperature=temp,EffectiveExpiry=expiry});
    }
    public async Task<Guid> Handle(ReceiveBankUnitCommand command,CancellationToken ct)
    {
        var user=await BankAccess.Require(roles,auth,"post","/Receive",ct);var v=command.Input;Validate(new ReceiveUnitValidator(),v);
        if(v.ReceivedAt.UtcDateTime>DateTime.UtcNow||v.ExpiresAt.UtcDateTime<=DateTime.UtcNow)throw Error("تاریخ دریافت یا انقضا معتبر نیست.");
        var code=BankValidation.Normalize(v.UnitCode);
        await using var tx=await Transaction(ct);
        if(await units.Table.IgnoreQueryFilters().AnyAsync(x=>x.OrgLocationGUID==user.OrgLocationGUID&&x.UnitCode==code,ct))throw Error("کد واحد قبلاً ثبت شده است.");
        var u=new BankBloodUnit{OrgLocationGUID=user.OrgLocationGUID,UnitCode=code,ProductCode=v.ProductCode,BloodGroup=v.BloodGroup,BatchNumber=v.BatchNumber,SupplySource=v.SupplySource.Trim(),EquipmentCode=BankValidation.Normalize(v.EquipmentCode),ReceivedAt=v.ReceivedAt.UtcDateTime,ExpiresAt=v.ExpiresAt.UtcDateTime,Status=BankRules.ReceivedHold};
        units.Table.Add(u);Event(u,user,"Receive","",null,v.Notes);await Save(ct);await tx.CommitAsync(ct);return u.GUID;
    }
    public async Task Handle(BankUnitActionCommand command,CancellationToken ct)
    {
        try { await ApplyUnitAction(command,ct); }
        catch (System.Data.Common.DbException)
        { throw Error("خطای عملیات دیتابیس بانک خون؛ اطلاعات را دوباره دریافت و بررسی کنید."); }
    }
    private async Task ApplyUnitAction(BankUnitActionCommand command,CancellationToken ct)
    {
        var v=command.Input;
        Validate(new UnitActionValidator(),v);
        var expected=v.Action is "StartThaw" or "FinishThaw"?"Thaw":v.Action;
        if(command.Endpoint!=expected)throw Error("عملیات نامعتبر است.");
        var user=await BankAccess.Require(roles,auth,"post","/"+expected,ct);
        await using var tx=await Transaction(ct);
        var u=await Unit(v.Guid,user,ct);Check(u.Version,v.Version);
        var before=u.Status;var request=u.BloodRequestGUID;var now=DateTime.UtcNow;
        var next=BankRules.Next(before,v.Action,BankRules.NeedsThaw(u.ProductCode),u.ThawCompletedAt.HasValue)??throw Error("عملیات در وضعیت جاری مجاز نیست.");
        if(v.Action is "Inspect" or "Reserve" or "StartThaw" or "FinishThaw" or "Ready"){Live(u);await TemperatureOk(u,ct);}
        switch(v.Action)
        {
            case "Inspect":
                if(request.HasValue||u.ThawStartedAt.HasValue)throw Error("واحد دارای رزرو یا سابقه ذوب به موجودی آزاد بازنمی‌گردد.");
                u.InspectionAt=now;break;
            case "Reserve":
                if(request.HasValue)throw Error("واحد قبلاً رزرو شده است.");
                var r=await Request(v.RequestGuid!.Value,user,ct);Check(r.Version,v.RequestVersion);Current(r);await LabOk(u,r,ct);
                var item=r.Items.SingleOrDefault(i=>i.ProductCode==u.ProductCode&&!i.IsDelete)??throw Error("فرآورده در اقلام درخواست وجود ندارد.");
                var allocated=await units.Table.CountAsync(x=>x.BloodRequestGUID==r.GUID&&x.ProductCode==u.ProductCode,ct);
                if(allocated>=item.Quantity)throw Error("تعداد رزرو بیش از مقدار تأییدشده است.");
                u.BloodRequestGUID=r.GUID;request=r.GUID;break;
            case "StartThaw":
            case "FinishThaw":
            case "Ready":
                if(!request.HasValue)throw Error("رزرو معتبر لازم است.");
                var active=await Request(request.Value,user,ct);Current(active);await LabOk(u,active,ct);
                if(v.Action=="StartThaw")u.ThawStartedAt=now;
                if(v.Action=="FinishThaw")
                {
                    var expiry=v.EffectiveExpiry!.Value.UtcDateTime;
                    if(!u.ThawStartedAt.HasValue||expiry<=now||expiry>u.ExpiresAt)throw Error("انقضای مستند پس از ذوب باید در محدوده انقضای قبلی و آینده باشد.");
                    u.ThawCompletedAt=now;u.ExpiresAt=expiry;
                }
                break;
            case "Unreserve":
                if(!request.HasValue)throw Error("واحد رزرو ندارد.");
                u.BloodRequestGUID=null;u.InspectionAt=null;break;
            case "Hold":
                u.InspectionAt=null;break;
            case "Discard":
                if(request.HasValue)throw Error("ابتدا رزرو واحد را لغو کنید.");
                u.InspectionAt=null;break;
        }
        u.Status=next;u.LastModified=now;Event(u,user,v.Action,before,request,v.Reason,v.Method,v.Temperature,v.EffectiveExpiry?.UtcDateTime);
        await Save(ct);await tx.CommitAsync(ct);
    }
    public async Task<Guid> Handle(RecordBankLabCommand command,CancellationToken ct)
    {
        var user=await BankAccess.Require(roles,auth,"post","/Test",ct);var v=command.Input;Validate(new LabInputValidator(),v);
        await using var tx=await Transaction(ct);
        var u=await Unit(v.UnitGuid,user,ct);Check(u.Version,v.UnitVersion);
        var r=await Request(v.RequestGuid,user,ct);Check(r.Version,v.RequestVersion);Current(r);Live(u);
        if(u.Status is not (BankRules.Available or BankRules.Reserved or BankRules.Ready)||u.BloodRequestGUID.HasValue&&u.BloodRequestGUID!=r.GUID||!r.Items.Any(x=>x.ProductCode==u.ProductCode&&!x.IsDelete))throw Error("واحد برای این درخواست قابل آزمایش نیست.");
        var t=new BankCompatibilityTest{BankBloodUnitGUID=u.GUID,BloodRequestGUID=r.GUID,SubmissionAttempt=r.SubmissionAttempt,RequestVersion=r.Version.ToArray(),AdmissionVersion=r.PatientAdmission.Version.ToArray(),ActorUserGUID=user.UserGuid,ActorRoleGUID=user.RoleGuid,RecordedAt=DateTime.UtcNow,SampleReference=v.SampleReference,SecondSample=v.SecondSample,LabelGroup=v.LabelGroup,Method=v.Method,ImmediateSpin=v.ImmediateSpin,Phase37=v.Phase37,Ahg=v.Ahg,Dat=v.Dat,AntibodyScreen=v.AntibodyScreen,ReportReference=v.ReportReference,Conclusion=v.Conclusion,Notes=v.Notes};
        if(t.Conclusion=="Accepted"&&(!BankRules.AcceptedLab(t)||t.LabelGroup!=u.BloodGroup||!u.InspectionAt.HasValue))throw Error("نتیجه پذیرفته‌شده با اطلاعات مستند آزمایش یا لیبل واحد مطابقت ندارد.");
        Db.BankCompatibilityTest.Add(t);
        var before=u.Status;
        if(t.Conclusion!="Accepted"||t.LabelGroup!=u.BloodGroup){u.Status=BankRules.Quarantined;u.InspectionAt=null;}
        else if(u.Status==BankRules.Ready)u.Status=BankRules.Reserved;
        u.LastModified=DateTime.UtcNow;Event(u,user,"Test",before,r.GUID,v.Notes);
        await Save(ct);await tx.CommitAsync(ct);return t.GUID;
    }
    public async Task<Guid> Handle(RecordBankTemperatureCommand command,CancellationToken ct)
    {
        var user=await BankAccess.Require(roles,auth,"post","/Temperature",ct);var v=command.Input;Validate(new TemperatureInputValidator(),v);
        var now=DateTime.UtcNow;if(v.MeasuredAt.UtcDateTime>now)throw Error("زمان اندازه‌گیری در آینده است.");
        var code=BankValidation.Normalize(v.EquipmentCode);
        await using var tx=await Transaction(ct);
        var log=new BankTemperatureLog{OrgLocationGUID=user.OrgLocationGUID,EquipmentCode=code,MeasuredAt=v.MeasuredAt.UtcDateTime,RecordedAt=now,Temperature=v.Temperature,Assessment=v.Assessment,Notes=v.Notes,ActorUserGUID=user.UserGuid,ActorRoleGUID=user.RoleGuid};
        Db.BankTemperatureLog.Add(log);
        // A reported concern is a conservative administrative hold, not an automated
        // clinical temperature assessment. Releasing the hold needs a new inspection.
        if(v.Assessment!="Acceptable")
        {
            var affected=await units.Table.Where(u=>u.OrgLocationGUID==user.OrgLocationGUID&&u.EquipmentCode==code&&u.Status!=BankRules.Discarded&&u.Status!=BankRules.Quarantined).ToListAsync(ct);
            foreach(var u in affected){var before=u.Status;u.Status=BankRules.Quarantined;u.InspectionAt=null;u.LastModified=now;Event(u,user,"TemperatureHold",before,u.BloodRequestGUID,v.Notes,null,v.Temperature);}
        }
        await Save(ct);await tx.CommitAsync(ct);return log.GUID;
    }
    public async Task Handle(BankReturnRequestCommand command,CancellationToken ct)
    {
        var user=await BankAccess.Require(roles,auth,"post","/ReturnRequest",ct);var v=command.Input;Validate(new ReturnRequestInputValidator(),v);
        await using var tx=await Transaction(ct);
        var r=await Request(v.RequestGuid,user,ct);Check(r.Version,v.Version);
        if(r.Status!="AwaitingBank"||await units.Table.AnyAsync(x=>x.BloodRequestGUID==r.GUID,ct))throw Error("درخواست در صف بانک نیست یا هنوز واحد رزرو دارد.");
        r.Status="Returned";r.LastModified=DateTime.UtcNow;
        Db.RequestApproval.Add(new RequestApproval{BloodRequestGUID=r.GUID,ActorUserGUID=user.UserGuid,ActorRoleGUID=user.RoleGuid,Action="BankReturn",PreviousStatus="AwaitingBank",NewStatus="Returned",Reason=v.Reason,SubmissionAttempt=r.SubmissionAttempt,OccurredAt=DateTime.UtcNow,SnapshotJson=JsonSerializer.Serialize(new{r.FormType,r.Priority,r.SubmissionAttempt,AdmissionVersion=r.AdmissionVersionAtSubmission})});
        await Save(ct);await tx.CommitAsync(ct);
    }
}
