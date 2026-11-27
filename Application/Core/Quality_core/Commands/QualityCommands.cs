using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using System.Text.Json;
namespace Application.Core.Quality_core.Commands;
public record CreateQualityCommand(string Kind,QualityCreate Input):IRequest<Guid>;
public record EditQualityCommand(string Kind,QualityEdit Input):IRequest;
public record SaveQualityLineCommand(string Kind,QualityLineInput Input):IRequest<Guid>;
public record QualityReviewCommand(string Kind,string Action,QualityAction Input):IRequest;
public record ResolutionCommand(string Action,ResolutionProgress Input):IRequest;
public record SaveFacilityCommand(FacilityInput Input):IRequest;
public class QualityCommandHandler(HemoQualityRecordService records,UserRoleService roles,AuthService auth):
    IRequestHandler<CreateQualityCommand,Guid>,IRequestHandler<EditQualityCommand>,IRequestHandler<SaveQualityLineCommand,Guid>,IRequestHandler<QualityReviewCommand>,IRequestHandler<ResolutionCommand>,IRequestHandler<SaveFacilityCommand>
{
    private IApplicationDbContext Db=>records.db;
    private static ServiceExeption Error(string s)=>new(s);
    private static void Ensure(bool b,string s="اطلاعات معتبر نیست؛ فرم را بررسی کنید."){if(!b)throw Error(s);}
    private static void Check(byte[] actual,byte[]? version)=>Ensure(QualityRules.Version(version)&&actual.SequenceEqual(version!),"نسخه اطلاعات تغییر کرده است.");
    private static bool DbFailure(Exception e){for(Exception? x=e;x!=null;x=x.InnerException)if(x is System.Data.Common.DbException or DbUpdateException)return true;return false;}
    private async Task<T> Atomic<T>(Func<Task<T>> fn,CancellationToken ct)
    {try{await using var tx=await ((DbContext)Db).Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);var result=await fn();await records.SaveChangesAsync(ct);await tx.CommitAsync(ct);return result;}catch(Exception e)when(DbFailure(e)){throw Error("ثبت همزمان یا خطای دیتابیس؛ اطلاعات را مجدداً دریافت کنید.");}}
    private async Task<UserRoleInfo> Access(string kind,string action,CancellationToken ct){Ensure(QualityRules.Kinds.Contains(kind));return await QualityAccess.Require(roles,auth,"post","/"+kind+action,ct);}
    private async Task<HemoQualityRecord> Load(Guid id,string kind,UserRoleInfo a,CancellationToken ct)=>await records.Table.SingleOrDefaultAsync(x=>x.GUID==id&&x.OrgLocationGUID==a.OrgLocationGUID&&x.Kind==kind,ct)??throw Error("پرونده در محدوده مرکز و نوع مجاز یافت نشد.");
    private void Log(HemoQualityRecord r,UserRoleInfo a,string action,string previous,string notes,object? data=null)
    {r.LastModified=DateTime.UtcNow;Db.HemoQualityEvent.Add(new HemoQualityEvent{HemoQualityRecordGUID=r.GUID,ActorUserGUID=a.UserGuid,ActorRoleGUID=a.RoleGuid,Action=action,PreviousStatus=previous,NewStatus=r.Status,OccurredAt=DateTime.UtcNow,Notes=notes,EvidenceJson=data==null?null:JsonSerializer.Serialize(data)});}
    public async Task<Guid> Handle(CreateQualityCommand cmd,CancellationToken ct)
    {
        var a=await Access(cmd.Kind,"Create",ct);var v=cmd.Input;Ensure(v!=null&&QualityValidation.Header(cmd.Kind,v.Title,v.Ward,v.OccurredAt,v.Header));
        return await Atomic(async()=>{var r=new HemoQualityRecord{OrgLocationGUID=a.OrgLocationGUID,Kind=cmd.Kind,Number=cmd.Kind.ToUpperInvariant()+"-"+Guid.NewGuid().ToString("N").ToUpperInvariant(),Title=v.Title,Ward=v.Ward,OccurredAt=v.OccurredAt.UtcDateTime,CreatedByUserGUID=a.UserGuid,DetailsJson=JsonSerializer.Serialize(v.Header)};records.Table.Add(r);Log(r,a,"Create","","پیش‌نویس ایجاد شد.",v.Header);return await Task.FromResult(r.GUID);},ct);
    }
    public async Task Handle(EditQualityCommand cmd,CancellationToken ct)
    {
        var a=await Access(cmd.Kind,"Edit",ct);var v=cmd.Input;Ensure(v!=null&&QualityValidation.Header(cmd.Kind,v.Title,v.Ward,v.OccurredAt,v.Header)&&QualityRules.Text(v.Notes,4000));
        await Atomic(async()=>{var r=await Load(v.Guid,cmd.Kind,a,ct);Check(r.Version,v.Version);Ensure(r.Status=="Draft"&&r.CreatedByUserGUID==a.UserGuid,"فقط سازنده می‌تواند پیش‌نویس را اصلاح کند.");
        var lines=await Db.HemoQualityLine.Where(x=>x.HemoQualityRecordGUID==r.GUID).ToListAsync(ct);
        Ensure(lines.All(x=>QualityValidation.Line(cmd.Kind,x.Title,JsonSerializer.Deserialize<QualityLineData>(x.DetailsJson),x.OwnerUserGUID,x.DueAt.HasValue?new DateTimeOffset(DateTime.SpecifyKind(x.DueAt.Value,DateTimeKind.Utc)):null,v.OccurredAt.UtcDateTime)),"ردیف‌های موجود با تغییر تاریخ سازگار نیستند.");
        if(cmd.Kind=="Training")Ensure(lines.Count<=v.Header.Capacity,"ظرفیت کمتر از تعداد شرکت‌کننده است.");
        var previous=r.DetailsJson;r.Title=v.Title;r.Ward=v.Ward;r.OccurredAt=v.OccurredAt.UtcDateTime;r.DetailsJson=JsonSerializer.Serialize(v.Header);Log(r,a,"Edit",r.Status,v.Notes,new{Previous=previous,Current=v.Header});return true;},ct);
    }
    public async Task<Guid> Handle(SaveQualityLineCommand cmd,CancellationToken ct)
    {
        var a=await Access(cmd.Kind,"Line",ct);var v=cmd.Input;Ensure(v!=null);
        return await Atomic(async()=>{var r=await Load(v.Guid,cmd.Kind,a,ct);Check(r.Version,v.Version);Ensure(r.Status=="Draft"&&r.CreatedByUserGUID==a.UserGuid,"فقط سازنده در پیش‌نویس مجاز به ثبت ردیف است.");
        Ensure(QualityValidation.Line(cmd.Kind,v.Title,v.Data,v.OwnerUserGuid,v.DueAt,r.OccurredAt));
        if(v.OwnerUserGuid.HasValue)Ensure(await QualityOwnerScope.Eligible(Db,a.OrgLocationGUID,cmd.Kind=="Committee").AnyAsync(x=>x.UserGUID==v.OwnerUserGuid,ct),"مسئول یا شرکت‌کننده باید حساب فعال مجاز همین مرکز باشد.");
        var lines=Db.HemoQualityLine.Where(x=>x.HemoQualityRecordGUID==r.GUID);
        HemoQualityLine line;
        if(v.LineGuid.HasValue){line=await lines.SingleOrDefaultAsync(x=>x.GUID==v.LineGuid,ct)??throw Error("ردیف یافت نشد.");Check(line.Version,v.LineVersion);}
        else{if(cmd.Kind=="Training"){var header=JsonSerializer.Deserialize<QualityHeader>(r.DetailsJson)!;Ensure(await lines.CountAsync(ct)<header.Capacity,"ظرفیت دوره تکمیل است.");}
          line=new HemoQualityLine{HemoQualityRecordGUID=r.GUID};Db.HemoQualityLine.Add(line);}
        if(cmd.Kind=="Training")Ensure(!await lines.AnyAsync(x=>x.OwnerUserGUID==v.OwnerUserGuid&&x.GUID!=line.GUID,ct),"شرکت‌کننده تکراری است.");
        var previous=line.DetailsJson;line.Title=v.Title;line.DetailsJson=JsonSerializer.Serialize(v.Data);line.OwnerUserGUID=v.OwnerUserGuid;line.DueAt=v.DueAt?.UtcDateTime;line.LastModified=DateTime.UtcNow;Log(r,a,"SaveLine",r.Status,v.Title,new{LineGuid=line.GUID,Previous=previous,Current=v});return line.GUID;},ct);
    }
    public async Task Handle(QualityReviewCommand cmd,CancellationToken ct)
    {
        Ensure(cmd.Action is "Submit" or "Approve" or "Reject" or "Close");var a=await Access(cmd.Kind,cmd.Action=="Reject"?"Approve":cmd.Action,ct);var v=cmd.Input;Ensure(v!=null&&QualityRules.Text(v.Notes,4000));
        await Atomic(async()=>{var r=await Load(v.Guid,cmd.Kind,a,ct);Check(r.Version,v.Version);var before=r.Status;var next=QualityRules.Next(before,cmd.Action)??throw Error("عملیات در وضعیت جاری مجاز نیست.");
        var lines=await Db.HemoQualityLine.Where(x=>x.HemoQualityRecordGUID==r.GUID).ToListAsync(ct);
        if(cmd.Action=="Submit"){
            Ensure(r.CreatedByUserGUID==a.UserGuid,"فقط سازنده پرونده را ارسال می‌کند.");var h=JsonSerializer.Deserialize<QualityHeader>(r.DetailsJson)!;var data=lines.Select(x=>JsonSerializer.Deserialize<QualityLineData>(x.DetailsJson)!).ToList();
            Ensure(QualityValidation.CanSubmit(r.Kind,h,data,r.OccurredAt),"ردیف‌ها، تاریخ انجام و اطلاعات تکمیلی لازم است.");r.ReviewedBy=null;r.ReviewedAt=null;r.ReviewNotes=null;
        }else{
            Ensure(QualityRules.Independent(a.UserGuid,r.CreatedByUserGUID),"بازبین باید مستقل از سازنده باشد.");
            if(cmd.Action=="Approve"||cmd.Action=="Reject"){r.ReviewedBy=a.UserGuid;r.ReviewedAt=DateTime.UtcNow;r.ReviewNotes=v.Notes;}
            if(cmd.Action=="Approve")foreach(var l in lines){l.Status=r.Kind=="Committee"?"Open":"Verified";l.LastModified=DateTime.UtcNow;}
            if(cmd.Action=="Close")Ensure(r.Kind=="Committee"&&lines.Count>0&&lines.All(x=>x.Status=="Verified"),"همه مصوبات باید مستقل تأیید شده باشند.");
        }
        r.Status=next;Log(r,a,cmd.Action,before,v.Notes);return true;},ct);
    }
    public async Task Handle(ResolutionCommand cmd,CancellationToken ct)
    {
        Ensure(cmd.Action is "Progress" or "Verify" or "Return");var a=await Access("Committee",cmd.Action=="Progress"?"Progress":"Verify",ct);var v=cmd.Input;Ensure(v!=null&&v.Progress>=0&&v.Progress<=100&&QualityRules.Text(v.Evidence,4000));
        await Atomic(async()=>{var r=await Load(v.Guid,"Committee",a,ct);Check(r.Version,v.Version);Ensure(r.Status=="Approved","ابتدا صورتجلسه باید بازبینی و تأیید شود.");
        var l=await Db.HemoQualityLine.SingleOrDefaultAsync(x=>x.GUID==v.LineGuid&&x.HemoQualityRecordGUID==r.GUID,ct)??throw Error("مصوبه یافت نشد.");Check(l.Version,v.LineVersion);var before=l.Status;
        if(cmd.Action=="Progress"){Ensure(l.OwnerUserGUID==a.UserGuid&&l.Status is "Open" or "InProgress" or "Returned","فقط مسئول مصوبه مجاز است.");l.Progress=v.Progress;l.Status=v.Progress==100?"Submitted":"InProgress";l.Evidence=v.Evidence;l.VerifiedBy=null;l.VerifiedAt=null;}
        else{Ensure(l.Status=="Submitted"&&l.OwnerUserGUID!=a.UserGuid&&r.CreatedByUserGUID!=a.UserGuid,"تأیید مستقل لازم است.");l.Status=cmd.Action=="Verify"?"Verified":"Returned";l.VerifiedBy=a.UserGuid;l.VerifiedAt=DateTime.UtcNow;}
        l.LastModified=DateTime.UtcNow;Log(r,a,"Resolution"+cmd.Action,r.Status,v.Evidence,new{LineGuid=l.GUID,Previous=before,Current=l.Status,l.Progress});return true;},ct);
    }
    public async Task Handle(SaveFacilityCommand cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/FacilitySave",ct);var v=cmd.Input;
        Ensure(v!=null&&QualityRules.Text(v.FacilityName,200)&&QualityRules.Optional(v.AccreditationCode,100)&&QualityRules.Optional(v.Chair,200)&&QualityRules.Optional(v.Secretary,200)&&QualityRules.Optional(v.Officer,200)&&QualityRules.Optional(v.BloodCenter,200)&&QualityRules.Optional(v.BloodCenterCode,100)&&QualityRules.Optional(v.ReportFooter,1000)&&QualityRules.Text(v.Notes,4000));
        await Atomic(async()=>{var p=await Db.HemoFacilityProfile.SingleOrDefaultAsync(x=>x.OrgLocationGUID==a.OrgLocationGUID,ct);
        if(p==null){Ensure(v.Version==null,"نسخه اطلاعات معتبر نیست.");p=new HemoFacilityProfile{OrgLocationGUID=a.OrgLocationGUID};Db.HemoFacilityProfile.Add(p);}else Check(p.Version,v.Version);
        p.FacilityName=v.FacilityName;p.AccreditationCode=v.AccreditationCode;p.Chair=v.Chair;p.Secretary=v.Secretary;p.Officer=v.Officer;p.BloodCenter=v.BloodCenter;p.BloodCenterCode=v.BloodCenterCode;p.ReportFooter=v.ReportFooter;p.LastModified=DateTime.UtcNow;return true;},ct);
    }
}
