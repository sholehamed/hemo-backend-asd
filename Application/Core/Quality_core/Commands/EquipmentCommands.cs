using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using Application.Core.BloodBank_core;
using System.Text.Json;
namespace Application.Core.Quality_core.Commands;
public record CreateEquipmentCommand(EquipmentInput Input):IRequest<Guid>;
public record EditEquipmentCommand(EquipmentEdit Input):IRequest;
public record EquipmentServiceCommand(ServiceInput Input):IRequest;
public record EquipmentStateCommand(EquipmentActionInput Input,string Action):IRequest;
public record ReportEquipmentIncidentCommand(IncidentInput Input):IRequest<Guid>;
public record ResolveEquipmentIncidentCommand(IncidentResolve Input):IRequest;
public record EquipmentTemperatureCommand(EquipmentTemperatureInput Input):IRequest<Guid>;
public class EquipmentCommandHandler(HemoEquipmentService equipment,UserRoleService roles,AuthService auth):
    IRequestHandler<CreateEquipmentCommand,Guid>,IRequestHandler<EditEquipmentCommand>,IRequestHandler<EquipmentServiceCommand>,IRequestHandler<EquipmentStateCommand>,IRequestHandler<ReportEquipmentIncidentCommand,Guid>,IRequestHandler<ResolveEquipmentIncidentCommand>,IRequestHandler<EquipmentTemperatureCommand,Guid>
{
    private IApplicationDbContext Db=>equipment.db;
    private static ServiceExeption Error(string s)=>new(s);
    private static void Ensure(bool b,string s="اطلاعات معتبر نیست؛ فرم را بررسی کنید."){if(!b)throw Error(s);}
    private static void Check(byte[] actual,byte[]? version)=>Ensure(QualityRules.Version(version)&&actual.SequenceEqual(version!),"نسخه اطلاعات تغییر کرده است.");
    private static bool DbFailure(Exception e){for(Exception? x=e;x!=null;x=x.InnerException)if(x is System.Data.Common.DbException or DbUpdateException)return true;return false;}
    private async Task<T> Atomic<T>(Func<Task<T>> fn,CancellationToken ct)
    {try{await using var tx=await ((DbContext)Db).Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);var result=await fn();await equipment.SaveChangesAsync(ct);await tx.CommitAsync(ct);return result;}catch(Exception e)when(DbFailure(e)){throw Error("ثبت همزمان یا خطای دیتابیس؛ اطلاعات را مجدداً دریافت کنید.");}}
    private async Task<HemoEquipment> Load(Guid id,UserRoleInfo a,CancellationToken ct)=>await equipment.Table.SingleOrDefaultAsync(x=>x.GUID==id&&x.OrgLocationGUID==a.OrgLocationGUID,ct)??throw Error("تجهیز در مرکز جاری یافت نشد.");
    private Task<bool> OpenIncident(HemoEquipment e,CancellationToken ct)=>Db.HemoEquipmentIncident.AnyAsync(x=>x.HemoEquipmentGUID==e.GUID&&x.Status=="Open",ct);
    private IQueryable<BankBloodUnit> Stored(HemoEquipment e)=>Db.BankBloodUnit.Where(x=>x.OrgLocationGUID==e.OrgLocationGUID&&x.EquipmentCode==e.Code&&(x.Status=="ReceivedHold"||x.Status=="Available"||x.Status=="Reserved"||x.Status=="Thawing"||x.Status=="Ready"||x.Status=="Quarantined"));
    private void Log(HemoEquipment e,UserRoleInfo a,string kind,string previous,string notes,string? reference=null,string? provider=null,DateTime? due=null,object? data=null)
    {e.LastModified=DateTime.UtcNow;Db.HemoEquipmentEvent.Add(new HemoEquipmentEvent{HemoEquipmentGUID=e.GUID,ActorUserGUID=a.UserGuid,ActorRoleGUID=a.RoleGuid,Kind=kind,PreviousStatus=previous,NewStatus=e.Status,OccurredAt=DateTime.UtcNow,Notes=notes,Reference=reference,Provider=provider,DueAt=due,EvidenceJson=data==null?null:JsonSerializer.Serialize(data)});}
    private static bool Fields(string name,string category,string model,string ward,string? serial)=>QualityRules.Text(name,200)&&QualityRules.Categories.Contains(category)&&QualityRules.Text(model,200)&&QualityRules.Text(ward,120)&&QualityRules.Optional(serial,100);
    public async Task<Guid> Handle(CreateEquipmentCommand cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/EquipmentCreate",ct);var v=cmd.Input;
        Ensure(v!=null&&BankValidation.Code(v.Code)&&Fields(v.Name,v.Category,v.BrandModel,v.Ward,v.SerialNumber));
        return await Atomic(async()=>{var code=BankValidation.Normalize(v.Code);Ensure(!await equipment.Table.IgnoreQueryFilters().AnyAsync(x=>x.OrgLocationGUID==a.OrgLocationGUID&&x.Code==code,ct),"کد ثابت تجهیز قبلاً ثبت شده است.");
        var e=new HemoEquipment{OrgLocationGUID=a.OrgLocationGUID,Code=code,Name=v.Name.Trim(),Category=v.Category,BrandModel=v.BrandModel,Ward=v.Ward,SerialNumber=v.SerialNumber,Status="Hold"};equipment.Table.Add(e);Log(e,a,"Register","","شناسنامه ثبت شد؛ تأیید فنی مستقل لازم است.",data:v);return e.GUID;},ct);
    }
    public async Task Handle(EditEquipmentCommand cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/EquipmentEdit",ct);var v=cmd.Input;Ensure(v!=null&&Fields(v.Name,v.Category,v.BrandModel,v.Ward,v.SerialNumber)&&QualityRules.Text(v.Notes,4000));
        await Atomic(async()=>{var e=await Load(v.Guid,a,ct);Check(e.Version,v.Version);Ensure(e.Status=="Hold","فقط شناسنامه در توقف فنی قابل اصلاح است.");e.Name=v.Name;e.Category=v.Category;e.BrandModel=v.BrandModel;e.Ward=v.Ward;e.SerialNumber=v.SerialNumber;Log(e,a,"Edit",e.Status,v.Notes,data:v);return true;},ct);
    }
    public async Task Handle(EquipmentServiceCommand cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/EquipmentService",ct);var v=cmd.Input;
        Ensure(v!=null&&QualityRules.ServiceKinds.Contains(v.Kind)&&QualityRules.Text(v.Reference,200)&&QualityRules.Text(v.Provider,200)&&QualityRules.Text(v.Checks,4000)&&QualityRules.Text(v.Notes,4000)&&new[]{"Accepted","Rejected","Pending"}.Contains(v.Result)&&v.PerformedAt!=default&&v.PerformedAt.UtcDateTime<=DateTime.UtcNow&&(!v.DueAt.HasValue||v.DueAt>v.PerformedAt));
        if(v.Result=="Accepted"&&(v.Kind=="Calibration"||v.Kind=="Maintenance"))Ensure(v.DueAt.HasValue,"سررسید مستند سرویس لازم است.");
        await Atomic(async()=>{var e=await Load(v.Guid,a,ct);Check(e.Version,v.Version);Ensure(e.Status!="Archived");var before=e.Status;e.Status="Hold";
        if(v.Kind=="Calibration"&&v.Result=="Accepted"){Ensure(!e.CalibrationAt.HasValue||v.PerformedAt.UtcDateTime>=e.CalibrationAt.Value,"ثبت گواهی قدیمی نمی‌تواند جایگزین گواهی جاری شود.");e.CalibrationAt=v.PerformedAt.UtcDateTime;e.CalibrationDueAt=v.DueAt?.UtcDateTime;}
        if(v.Kind=="Maintenance"&&v.Result=="Accepted")e.MaintenanceDueAt=v.DueAt?.UtcDateTime;
        Log(e,a,v.Kind,before,v.Notes,v.Reference,v.Provider,v.DueAt?.UtcDateTime,v);return true;},ct);
    }
    public async Task Handle(EquipmentStateCommand cmd,CancellationToken ct)
    {
        Ensure(cmd.Action is "Hold" or "Release" or "Archive");var a=await QualityAccess.Require(roles,auth,"post","/Equipment"+cmd.Action,ct);var v=cmd.Input;Ensure(v!=null&&QualityRules.Text(v.Notes,4000)&&QualityRules.Optional(v.Reference,200));
        await Atomic(async()=>{var e=await Load(v.Guid,a,ct);Check(e.Version,v.Version);var before=e.Status;var incident=await OpenIncident(e,ct);
        if(cmd.Action=="Hold"){Ensure(e.Status=="Operational");e.Status="Hold";}
        if(cmd.Action=="Release"){
            var last=await Db.HemoEquipmentEvent.Where(x=>x.HemoEquipmentGUID==e.GUID&&(x.Kind=="Calibration"||x.Kind=="Maintenance"||x.Kind=="Repair"||x.Kind=="Qualification")).OrderByDescending(x=>x.OccurredAt).ThenByDescending(x=>x.GUID).FirstOrDefaultAsync(ct);
            var barrier=await Db.HemoEquipmentEvent.Where(x=>x.HemoEquipmentGUID==e.GUID&&(x.Kind=="IncidentReport"||x.Kind=="IncidentResolve"||x.Kind=="Hold"||x.Kind=="Edit"||x.Kind=="Temperature"&&x.NewStatus=="Hold")).MaxAsync(x=>(DateTime?)x.OccurredAt,ct);
            var accepted=false;if(last?.EvidenceJson!=null&&(!barrier.HasValue||last.OccurredAt>=barrier.Value)){using var json=JsonDocument.Parse(last.EvidenceJson);accepted=json.RootElement.TryGetProperty("Result",out var result)&&result.GetString()=="Accepted";}
            Ensure(QualityRules.CanRelease(e.Status,incident,accepted&&last!.ActorUserGUID!=a.UserGuid)&&QualityRules.Text(v.Reference,200)&&(!e.CalibrationDueAt.HasValue||e.CalibrationDueAt>DateTime.UtcNow)&&(!e.MaintenanceDueAt.HasValue||e.MaintenanceDueAt>DateTime.UtcNow),"گواهی معتبر، تأیید مستقل، نبود حادثه باز و سررسید معتبر لازم است.");e.Status="Operational";
        }
        if(cmd.Action=="Archive"){Ensure(QualityRules.CanArchive(e.Status,await Stored(e).AnyAsync(ct),incident),"تجهیز دارای واحد ذخیره‌شده یا حادثه باز قابل آرشیو نیست.");e.Status="Archived";}
        Log(e,a,cmd.Action,before,v.Notes,v.Reference);return true;},ct);
    }
    public async Task<Guid> Handle(ReportEquipmentIncidentCommand cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/IncidentReport",ct);var v=cmd.Input;Ensure(v!=null&&QualityRules.Urgencies.Contains(v.Urgency)&&QualityRules.Text(v.Description,4000));
        return await Atomic(async()=>{var e=await Load(v.Guid,a,ct);Check(e.Version,v.Version);Ensure(e.Status!="Archived");var before=e.Status;e.Status="Hold";
        var i=new HemoEquipmentIncident{HemoEquipmentGUID=e.GUID,IncidentNumber="EQI-"+Guid.NewGuid().ToString("N").ToUpperInvariant(),Urgency=v.Urgency,Status="Open",ReportedAt=DateTime.UtcNow,ReporterUserGUID=a.UserGuid,Description=v.Description};Db.HemoEquipmentIncident.Add(i);Log(e,a,"IncidentReport",before,v.Description,i.IncidentNumber,data:new{IncidentGuid=i.GUID,v.Urgency});return i.GUID;},ct);
    }
    public async Task Handle(ResolveEquipmentIncidentCommand cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/IncidentResolve",ct);var v=cmd.Input;Ensure(v!=null&&QualityRules.Text(v.Resolution,4000)&&QualityRules.Text(v.Reference,200));
        await Atomic(async()=>{var i=await Db.HemoEquipmentIncident.Include(x=>x.Equipment).SingleOrDefaultAsync(x=>x.GUID==v.Guid&&x.Equipment.OrgLocationGUID==a.OrgLocationGUID,ct)??throw Error("حادثه در مرکز یافت نشد.");Check(i.Version,v.Version);Check(i.Equipment.Version,v.EquipmentVersion);Ensure(i.Status=="Open");i.Status="Resolved";i.Resolution=v.Resolution;i.ResolutionReference=v.Reference;i.ResolvedAt=DateTime.UtcNow;i.ResolvedBy=a.UserGuid;i.LastModified=DateTime.UtcNow;Log(i.Equipment,a,"IncidentResolve",i.Equipment.Status,v.Resolution,v.Reference,data:new{IncidentGuid=i.GUID});return true;},ct);
    }
    public async Task<Guid> Handle(EquipmentTemperatureCommand cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/EquipmentTemperature",ct);var v=cmd.Input;
        Ensure(v!=null&&new[]{"Morning","Evening","Night","Other"}.Contains(v.Shift)&&BankValidation.Temperature(v.Temperature)&&BankValidation.Temperature(v.Minimum)&&BankValidation.Temperature(v.Maximum)&&v.Minimum<=v.Temperature&&v.Temperature<=v.Maximum&&v.MeasuredAt!=default&&v.MeasuredAt.UtcDateTime<=DateTime.UtcNow&&BankRules.Assessments.Contains(v.Assessment)&&QualityRules.Text(v.Notes,2000));
        return await Atomic(async()=>{var e=await Load(v.Guid,a,ct);Check(e.Version,v.Version);Ensure(e.Status!="Archived");var before=e.Status;var now=DateTime.UtcNow;
        var log=new BankTemperatureLog{OrgLocationGUID=a.OrgLocationGUID,EquipmentCode=e.Code,MeasuredAt=v.MeasuredAt.UtcDateTime,RecordedAt=now,Temperature=v.Temperature,Assessment=v.Assessment,Notes=v.Notes,ActorUserGUID=a.UserGuid,ActorRoleGUID=a.RoleGuid};Db.BankTemperatureLog.Add(log);
        if(v.Assessment!="Acceptable"){e.Status="Hold";foreach(var u in await Stored(e).Where(x=>x.Status!="Quarantined").ToListAsync(ct)){var old=u.Status;u.Status="Quarantined";u.InspectionAt=null;u.LastModified=now;Db.BankUnitEvent.Add(new BankUnitEvent{BankBloodUnitGUID=u.GUID,BloodRequestGUID=u.BloodRequestGUID,ActorUserGUID=a.UserGuid,ActorRoleGUID=a.RoleGuid,Action="TemperatureHold",PreviousStatus=old,NewStatus=u.Status,OccurredAt=now,Reason=v.Notes,Temperature=v.Temperature});}}
        Log(e,a,"Temperature",before,v.Notes,data:new{TemperatureLogGuid=log.GUID,Input=v});return log.GUID;},ct);
    }
}
