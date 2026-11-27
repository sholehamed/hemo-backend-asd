using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using System.Text.Json;
namespace Application.Core.Hemovigilance_core.Commands;
public record ReportReactionCommand(ReactionInput Input):IRequest<Guid>;
public record ReviewReactionCommand(ReviewInput Input,string Action):IRequest;
public record AssignCapaCommand(CapaCreateInput Input):IRequest<Guid>;
public record CapaActionCommand(CapaActionInput Input,string Action):IRequest;
public record ArchiveHvReportCommand(ReportInput Input):IRequest<Guid>;
public record RecordReportExternalCommand(ReportExternalInput Input):IRequest;
public class HvCommandHandler(HemoReactionService cases,UserRoleService roles,AuthService auth):
    IRequestHandler<ReportReactionCommand,Guid>,IRequestHandler<ReviewReactionCommand>,IRequestHandler<AssignCapaCommand,Guid>,IRequestHandler<CapaActionCommand>,IRequestHandler<ArchiveHvReportCommand,Guid>,IRequestHandler<RecordReportExternalCommand>
{
    private IApplicationDbContext Db=>cases.db;
    private static ServiceExeption Error(string message)=>new(message);
    private static void Validate<T>(AbstractValidator<T> validator,T value){if(value==null||!validator.Validate(value).IsValid)throw Error("اطلاعات عملیات معتبر نیست؛ فرم را بررسی کنید.");}
    private static void Check(byte[] actual,byte[] version){if(version==null||!actual.SequenceEqual(version))throw Error("نسخه اطلاعات تغییر کرده است؛ دوباره دریافت کنید.");}
    private static bool DatabaseFailure(Exception? e){for(var x=e;x!=null;x=x.InnerException)if(x is System.Data.Common.DbException or DbUpdateException)return true;return false;}
    private async Task<T> Atomic<T>(Func<Task<T>> action,CancellationToken ct)
    {
        try{
            await using var tx=await ((DbContext)Db).Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
            var result=await action();await cases.SaveChangesAsync(ct);await tx.CommitAsync(ct);return result;
        }catch(Exception e) when(DatabaseFailure(e)){throw Error("ثبت همزمان یا خطای دیتابیس؛ اطلاعات را دوباره دریافت و بررسی کنید.");}
    }
    private IQueryable<HemoReaction> Scope(UserRoleInfo a)=>cases.Table.Where(x=>x.OrgLocationGUID==a.OrgLocationGUID&&x.Transfer.OrgLocationGUID==a.OrgLocationGUID&&x.Transfer.Admission.OrgLocationGUID==a.OrgLocationGUID&&x.Transfer.Unit.OrgLocationGUID==a.OrgLocationGUID);
    private async Task<HemoReaction> Load(Guid id,UserRoleInfo a,CancellationToken ct)=>await Scope(a).SingleOrDefaultAsync(x=>x.GUID==id,ct)??throw Error("پرونده در محدوده مرکز یافت نشد.");
    private static bool Blank(string? s)=>string.IsNullOrWhiteSpace(s);
    private void Log(HemoReaction r,UserRoleInfo a,string action,string before,string? notes,object? evidence=null)
    {Db.HemoReactionEvent.Add(new HemoReactionEvent{HemoReactionGUID=r.GUID,ActorUserGUID=a.UserGuid,ActorRoleGUID=a.RoleGuid,Action=action,PreviousStatus=before,NewStatus=r.Status,OccurredAt=DateTime.UtcNow,Notes=notes,EvidenceJson=evidence==null?null:JsonSerializer.Serialize(evidence)});}
    public async Task<Guid> Handle(ReportReactionCommand cmd,CancellationToken ct)
    {
        var a=await HvAccess.Require(roles,auth,"post","/Report",ct);var v=cmd.Input;Validate(new ReactionInputValidator(),v);
        return await Atomic(async()=>{
            var t=await Db.BloodTransfer.SingleOrDefaultAsync(x=>x.GUID==v.TransferGuid&&x.OrgLocationGUID==a.OrgLocationGUID&&x.Admission.OrgLocationGUID==a.OrgLocationGUID&&x.Unit.OrgLocationGUID==a.OrgLocationGUID,ct)??throw Error("حواله در محدوده مجاز نیست.");
            var existing=await Scope(a).SingleOrDefaultAsync(x=>x.ClientReportId==v.ClientReportId,ct);
            if(existing!=null){if(existing.BloodTransferGUID!=t.GUID||existing.ReporterUserGUID!=a.UserGuid)throw Error("شناسه ثبت قبلاً استفاده شده است.");return existing.GUID;}
            Check(t.Version,v.TransferVersion);var now=DateTime.UtcNow;
            if(!HvRules.CanReport(t)||v.OnsetAt.UtcDateTime<t.StartedAt!.Value||v.OnsetAt.UtcDateTime>now)throw Error("زمان مشاهده باید بعد از شروع ثبت‌شده و حداکثر زمان جاری باشد.");
            if(await Scope(a).AnyAsync(x=>x.BloodTransferGUID==t.GUID&&x.Status!="Closed",ct))throw Error("برای این حواله پرونده باز وجود دارد؛ مشاهده جدید را در آن ثبت کنید.");
            var r=new HemoReaction{OrgLocationGUID=a.OrgLocationGUID,BloodTransferGUID=t.GUID,ClientReportId=v.ClientReportId,CaseNumber="HV-"+Guid.NewGuid().ToString("N").ToUpperInvariant(),Status="Reported",OnsetAt=v.OnsetAt.UtcDateTime,ReportedAt=now,ReporterUserGUID=a.UserGuid,SymptomsJson=JsonSerializer.Serialize(v.Symptoms),InitialSeverity=v.InitialSeverity,InitialNotes=v.Notes,ActionsTaken=v.ActionsTaken,SamplesSent=v.SamplesSent,VolumeMl=v.VolumeMl};
            cases.Table.Add(r);Log(r,a,"Report","",v.Notes,v);
            t.SuspectedReaction=true;t.LastModified=now;
            Db.BloodTransferEvent.Add(new BloodTransferEvent{BloodTransferGUID=t.GUID,ActorUserGUID=a.UserGuid,ActorRoleGUID=a.RoleGuid,Action="ReactionReport",PreviousStatus=t.Status,NewStatus=t.Status,OccurredAt=now,Notes="پرونده نگرانی ثبت شد: "+r.CaseNumber,EvidenceJson=JsonSerializer.Serialize(new{ReactionGuid=r.GUID})});
            return r.GUID;
        },ct);
    }
    public async Task Handle(ReviewReactionCommand cmd,CancellationToken ct)
    {
        var action=cmd.Action;if(!new[]{"Observe","DoctorReview","BankReview","Investigate","ReturnToDoctor","PublishRca","Close","External"}.Contains(action))throw Error("عملیات نامعتبر است.");
        var a=await HvAccess.Require(roles,auth,"post","/"+action,ct);var v=cmd.Input;Validate(new ReviewInputValidator(),v);
        await Atomic(async()=>{
            var r=await Load(v.Guid,a,ct);Check(r.Version,v.Version);var before=r.Status;var now=DateTime.UtcNow;
            if(before=="Closed")throw Error("پرونده بسته است؛ برای رخداد مستقل پرونده جدید ثبت شود.");
            var next=action is "Observe" or "External"?before:HvRules.Next(before,action,v.RcaRequired==true)??throw Error("عملیات در وضعیت جاری مجاز نیست.");
            switch(action)
            {
                case "Observe": break; // append-only observation, never rewrite the original
                case "DoctorReview":
                    if(!HvRules.Diagnoses.Contains(v.DiagnosisCode)||!HvRules.Severities.Contains(v.Severity)||!HvRules.Imputabilities.Contains(v.Imputability)||!HvRules.Outcomes.Contains(v.Outcome)||Blank(v.DoctorReference)||Blank(v.Actions))throw Error("طبقه‌بندی مستند پزشک، پیامد، مرجع و شرح اقدامات لازم است.");
                    r.DoctorUserGUID=a.UserGuid;r.DoctorReviewedAt=now;r.DiagnosisCode=v.DiagnosisCode;r.Severity=v.Severity;r.Imputability=v.Imputability;r.Outcome=v.Outcome;r.DoctorNotes=v.Notes;r.DoctorActions=v.Actions;r.DoctorLabRequest=v.LabRequest;r.FutureRecommendations=v.Recommendations;r.DoctorReference=v.DoctorReference;break;
                case "BankReview":
                    if(Blank(v.SampleReference)||Blank(v.ReportReference)||Blank(v.Disposition))throw Error("وضعیت نمونه، مرجع گزارش و نتیجه بررسی ثبت شود (عدم دسترسی نیز مستند شود).");
                    r.BankUserGUID=a.UserGuid;r.BankReviewedAt=now;r.BankFindings=v.Notes;r.BankSampleReference=v.SampleReference;r.BankReportReference=v.ReportReference;r.BankDisposition=v.Disposition;break;
                case "Investigate":
                    if(!v.RcaRequired.HasValue||!HvRules.ExternalStatuses.Contains(v.ExternalStatus)||v.ExternalStatus=="NotRecorded"||v.ExternalStatus=="SentManually"&&Blank(v.ExternalReference))throw Error("تصمیم مستند RCA و وضعیت گزارش بیرونی لازم است.");
                    r.InvestigationNotes=v.Notes;r.RcaRequired=v.RcaRequired;r.ExternalStatus=v.ExternalStatus!;r.ExternalReference=v.ExternalStatus=="SentManually"?v.ExternalReference:null;break;
                case "ReturnToDoctor":
                    if(await Db.HemoCapa.AnyAsync(x=>x.HemoReactionGUID==r.GUID,ct))throw Error("پرونده دارای اقدام اصلاحی است؛ بازگردانی در این وضعیت مجاز نیست.");
                    r.DoctorUserGUID=null;r.DoctorReviewedAt=null;r.DiagnosisCode=null;r.Severity=null;r.Imputability=null;r.Outcome=null;r.DoctorNotes=null;r.DoctorActions=null;r.DoctorLabRequest=null;r.FutureRecommendations=null;r.DoctorReference=null;
                    r.BankUserGUID=null;r.BankReviewedAt=null;r.BankFindings=null;r.BankSampleReference=null;r.BankReportReference=null;r.BankDisposition=null;r.InvestigationNotes=null;r.RcaRequired=null;r.ExternalStatus="NotRecorded";r.ExternalReference=null;break;
                case "PublishRca":
                    if(Blank(v.RcaLead)||!HvRules.Methods.Contains(v.RcaMethod)||!v.MeetingAt.HasValue||v.MeetingAt.Value.UtcDateTime>now||v.MeetingAt.Value.UtcDateTime<r.OnsetAt||Blank(v.Participants)||Blank(v.DirectCause)||Blank(v.RootCause)||!await Db.HemoCapa.AnyAsync(x=>x.HemoReactionGUID==r.GUID,ct))throw Error("اطلاعات کامل جلسه، علل مستند و حداقل یک اقدام اصلاحی/پیشگیرانه لازم است.");
                    r.RcaLead=v.RcaLead;r.RcaMethod=v.RcaMethod;r.RcaMeetingAt=v.MeetingAt.Value.UtcDateTime;r.RcaParticipants=v.Participants;r.DirectCause=v.DirectCause;r.RootCause=v.RootCause;r.RcaPublishedAt=now;r.RcaPublishedBy=a.UserGuid;break;
                case "External":
                    if(!HvRules.ExternalStatuses.Contains(v.ExternalStatus)||v.ExternalStatus=="NotRecorded"||v.ExternalStatus=="SentManually"&&Blank(v.ExternalReference))throw Error("وضعیت و مرجع گزارش واقعی لازم است.");
                    r.ExternalStatus=v.ExternalStatus!;r.ExternalReference=v.ExternalStatus=="SentManually"?v.ExternalReference:null;break;
                case "Close":
                    var actions=await Db.HemoCapa.Where(x=>x.HemoReactionGUID==r.GUID).ToListAsync(ct);
                    if(await Db.BloodTransfer.AnyAsync(t=>t.GUID==r.BloodTransferGUID&&t.Status=="InProgress",ct)||!HvRules.CloseAllowed(r,actions)||!HvRules.ExternalComplete(r.ExternalStatus,r.ExternalReference)||a.UserGuid==r.ReporterUserGUID||a.UserGuid==r.RcaPublishedBy||await Db.HemoReactionEvent.AnyAsync(e=>e.HemoReactionGUID==r.GUID&&e.Action=="Investigate"&&e.ActorUserGUID==a.UserGuid,ct))throw Error("تکمیل بررسی‌ها، تأیید مستقل اقدامات و تعیین تکلیف گزارش لازم است.");
                    r.ClosedAt=now;r.ClosedBy=a.UserGuid;break;
            }
            r.Status=next;r.LastModified=now;Log(r,a,action,before,v.Notes,v);return true;
        },ct);
    }
    public async Task<Guid> Handle(AssignCapaCommand cmd,CancellationToken ct)
    {
        var a=await HvAccess.Require(roles,auth,"post","/AssignCapa",ct);var v=cmd.Input;Validate(new CapaCreateValidator(),v);
        return await Atomic(async()=>{
            var r=await Load(v.Guid,a,ct);Check(r.Version,v.Version);
            if(r.Status!="RcaOpen"&&r.Status!="CapaMonitoring"||v.DueAt.UtcDateTime<=DateTime.UtcNow)throw Error("پرونده RCA باز و موعد آینده لازم است.");
            var owner=await HvOwnerQuery.Eligible(Db,a.OrgLocationGUID).Where(x=>x.UserGUID==v.OwnerUserGuid).Select(x=>new{x.UserGUID,Name=x.User.FirstName+" "+x.User.LastName}).FirstOrDefaultAsync(ct)??throw Error("مسئول اجرا باید کاربر مجاز فعال همین مرکز باشد.");
            var c=new HemoCapa{HemoReactionGUID=r.GUID,Title=v.Title,Kind=v.Kind,OwnerUserGUID=owner.UserGUID,OwnerName=owner.Name,DueAt=v.DueAt.UtcDateTime,Indicator=v.Indicator,Status="Open"};
            Db.HemoCapa.Add(c);r.LastModified=DateTime.UtcNow;Log(r,a,"AssignCapa",r.Status,v.Title,new{CapaGuid=c.GUID,v.OwnerUserGuid,v.DueAt,v.Indicator,v.Kind});return c.GUID;
        },ct);
    }
    public async Task Handle(CapaActionCommand cmd,CancellationToken ct)
    {
        if(cmd.Action is not ("CapaProgress" or "CapaVerify" or "CapaReject"))throw Error("عملیات نامعتبر است.");
        var a=await HvAccess.Require(roles,auth,"post",cmd.Action=="CapaReject"?"/CapaVerify":"/"+cmd.Action,ct);var v=cmd.Input;Validate(new CapaActionValidator(),v);
        await Atomic(async()=>{
            var c=await Db.HemoCapa.Include(x=>x.Reaction).SingleOrDefaultAsync(x=>x.GUID==v.Guid&&Scope(a).Any(r=>r.GUID==x.HemoReactionGUID),ct)??throw Error("اقدام در مرکز یافت نشد.");
            var r=c.Reaction;Check(c.Version,v.Version);Check(r.Version,v.CaseVersion);var now=DateTime.UtcNow;
            if(r.Status!="CapaMonitoring")throw Error("ابتدا RCA منتشر شود؛ پرونده بسته قابل تغییر نیست.");
            var previous=c.Status;
            if(cmd.Action=="CapaProgress")
            {
                if(c.OwnerUserGUID!=a.UserGuid||c.Status is not ("Open" or "InProgress" or "Rejected"))throw Error("فقط مسئول اجرا در وضعیت مجاز می‌تواند گزارش ثبت کند.");
                c.Progress=v.Progress;c.Evidence=v.Evidence;c.Status=v.Progress==100?"Submitted":"InProgress";c.SubmittedAt=v.Progress==100?now:null;c.SubmittedBy=v.Progress==100?a.UserGuid:null;c.VerifiedAt=null;c.VerifiedBy=null;c.VerificationNotes=null;
            }else{
                if(c.Status!="Submitted"||!HvRules.Independent(a.UserGuid,c.OwnerUserGUID,c.SubmittedBy))throw Error("اقدام ارسال‌شده و بازبین مستقل لازم است.");
                c.Status=cmd.Action=="CapaVerify"?"Verified":"Rejected";c.VerifiedAt=now;c.VerifiedBy=a.UserGuid;c.VerificationNotes=v.Evidence;
            }
            c.LastModified=now;r.LastModified=now;Log(r,a,cmd.Action,r.Status,v.Evidence,new{CapaGuid=c.GUID,Previous=previous,Current=c.Status,c.Progress});return true;
        },ct);
    }
    public async Task<Guid> Handle(ArchiveHvReportCommand cmd,CancellationToken ct)
    {
        var a=await HvAccess.Require(roles,auth,"post","/Archive",ct);var v=cmd.Input;Validate(new ReportInputValidator(),v);
        return await Atomic(async()=>{
            var summary=await HvReportQuery.Summary(Db,a.OrgLocationGUID,new HvFilters(Ward:v.Ward,ProductCode:v.ProductCode,From:v.From,To:v.To),ct);
            var report=new HemoReport{OrgLocationGUID=a.OrgLocationGUID,ReportNumber="HVR-"+Guid.NewGuid().ToString("N").ToUpperInvariant(),Title=v.Title,FromAt=v.From.UtcDateTime,ToAt=v.To.UtcDateTime,Ward=v.Ward,ProductCode=v.ProductCode,PreparedBy=a.UserGuid,PreparedAt=DateTime.UtcNow,SnapshotJson=JsonSerializer.Serialize(summary)};
            Db.HemoReport.Add(report);return report.GUID;
        },ct);
    }
    public async Task Handle(RecordReportExternalCommand cmd,CancellationToken ct)
    {
        var a=await HvAccess.Require(roles,auth,"post","/ReportExternal",ct);var v=cmd.Input;
        if(v==null||v.Guid==Guid.Empty||v.Version?.Length!=8||!HvRules.ExternalStatuses.Contains(v.Status)||v.Status=="NotRecorded"||v.Status=="SentManually"&&Blank(v.Reference)||v.Reference?.Length>200)throw Error("وضعیت و مرجع مستند لازم است.");
        await Atomic(async()=>{
            var r=await Db.HemoReport.SingleOrDefaultAsync(x=>x.GUID==v.Guid&&x.OrgLocationGUID==a.OrgLocationGUID,ct)??throw Error("گزارش یافت نشد.");Check(r.Version,v.Version);
            r.ExternalStatus=v.Status;r.ExternalReference=v.Status=="SentManually"?v.Reference:null;r.ExternalRecordedBy=a.UserGuid;r.ExternalRecordedAt=DateTime.UtcNow;r.LastModified=DateTime.UtcNow;return true;
        },ct);
    }
}
