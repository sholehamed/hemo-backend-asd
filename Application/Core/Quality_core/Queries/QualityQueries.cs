using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using Application.Core.PatientAdmission_core;
using System.Linq.Expressions;
using System.Text.Json;
namespace Application.Core.Quality_core.Queries;
public record QualityListQuery(string Kind,QualityFilter Filter):IRequest<PaginatedList<QualityRow>>;
public record QualityDetailQuery(string Kind,Guid Guid):IRequest<QualityDetail>;
public record QualityOwnersQuery(string Kind):IRequest<List<QualityOwner>>;
public record QualityCapabilitiesQuery:IRequest<QualityCapabilities>;
public record FacilityQuery:IRequest<FacilityView>;
public record QualityStatsQuery(QualityStatsFilter Filter):IRequest<QualityStats>;
public class QualityQueryHandler(HemoQualityRecordService records,UserRoleService roles,AuthService auth):
    IRequestHandler<QualityListQuery,PaginatedList<QualityRow>>,IRequestHandler<QualityDetailQuery,QualityDetail>,IRequestHandler<QualityOwnersQuery,List<QualityOwner>>,IRequestHandler<QualityCapabilitiesQuery,QualityCapabilities>,IRequestHandler<FacilityQuery,FacilityView>,IRequestHandler<QualityStatsQuery,QualityStats>
{
    private IApplicationDbContext Db=>records.db;
    private static Expression<Func<HemoQualityRecord,QualityRow>> Row()=>r=>new(r.GUID,r.Kind,r.Number,r.Title,r.Ward,r.Status,r.OccurredAt,r.CreatedByUserGUID,r.ReviewedAt,r.Version,r.Number.StartsWith("DEMO-"));
    private IQueryable<HemoQualityRecord> Scope(Guid org,string kind)=>records.Table.AsNoTracking().Where(x=>x.OrgLocationGUID==org&&x.Kind==kind);
    public async Task<PaginatedList<QualityRow>> Handle(QualityListQuery cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/"+cmd.Kind+"List",ct);var f=cmd.Filter??new();var q=Scope(a.OrgLocationGUID,cmd.Kind);
        if(!string.IsNullOrWhiteSpace(f.Search)){var s=f.Search.Trim();q=q.Where(x=>x.Number.Contains(s)||x.Title.Contains(s));}
        if(!string.IsNullOrWhiteSpace(f.Ward))q=q.Where(x=>x.Ward==f.Ward);
        if(!string.IsNullOrWhiteSpace(f.Status))q=q.Where(x=>x.Status==f.Status);
        if(f.From.HasValue)q=q.Where(x=>x.OccurredAt>=f.From.Value.UtcDateTime);if(f.To.HasValue)q=q.Where(x=>x.OccurredAt<=f.To.Value.UtcDateTime);
        if(f.Mine)q=q.Where(x=>Db.HemoQualityLine.Any(l=>l.HemoQualityRecordGUID==x.GUID&&l.OwnerUserGUID==a.UserGuid));
        var p=Math.Clamp(f.PageNumber,1,100000);var size=Math.Clamp(f.PageSize,1,100);return new(await q.OrderByDescending(x=>x.OccurredAt).ThenBy(x=>x.GUID).Skip((p-1)*size).Take(size).Select(Row()).ToListAsync(ct),await q.CountAsync(ct),p,size);
    }
    public async Task<QualityDetail> Handle(QualityDetailQuery cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"get","/"+cmd.Kind+"Detail/{guid}",ct);var r=await Scope(a.OrgLocationGUID,cmd.Kind).SingleOrDefaultAsync(x=>x.GUID==cmd.Guid,ct)??throw new ServiceExeption("پرونده یافت نشد.");
        var row=new QualityRow(r.GUID,r.Kind,r.Number,r.Title,r.Ward,r.Status,r.OccurredAt,r.CreatedByUserGUID,r.ReviewedAt,r.Version,r.Number.StartsWith("DEMO-"));
        var lines=await Db.HemoQualityLine.AsNoTracking().Where(x=>x.HemoQualityRecordGUID==r.GUID).OrderBy(x=>x.Created).ThenBy(x=>x.GUID).ToListAsync(ct);
        var views=lines.Select(x=>new QualityLineView(x.GUID,x.Title,JsonSerializer.Deserialize<QualityLineData>(x.DetailsJson)!,x.Status,x.OwnerUserGUID,x.DueAt,x.Progress,x.Evidence,x.VerifiedBy,x.VerifiedAt,x.Version)).ToList();
        var events=await Db.HemoQualityEvent.AsNoTracking().Where(x=>x.HemoQualityRecordGUID==r.GUID).OrderBy(x=>x.OccurredAt).ThenBy(x=>x.GUID).Select(x=>new QualityEventView(x.Action,x.PreviousStatus,x.NewStatus,x.OccurredAt,x.ActorUserGUID,x.ActorRoleGUID,x.Notes,x.EvidenceJson)).ToListAsync(ct);
        return new(row,JsonSerializer.Deserialize<QualityHeader>(r.DetailsJson)!,views,events,r.ReviewNotes,r.ReviewedBy,r.Kind=="Audit"?views.Sum(x=>x.Data.Score??0):null,r.Kind=="Audit"?views.Count*2:null);
    }
    public async Task<List<QualityOwner>> Handle(QualityOwnersQuery cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"get","/"+cmd.Kind+"Owners",ct);var rows=await QualityOwnerScope.Eligible(Db,a.OrgLocationGUID,cmd.Kind=="Committee").Select(x=>new{x.UserGUID,Name=x.User.FirstName+" "+x.User.LastName}).Distinct().OrderBy(x=>x.Name).ToListAsync(ct);return rows.Select(x=>new QualityOwner(x.UserGUID,x.Name)).ToList();
    }
    public async Task<QualityCapabilities> Handle(QualityCapabilitiesQuery cmd,CancellationToken ct)
    {
        var a=await AdmissionScope.GetAssignmentAsync(roles,auth,ct);var result=new Dictionary<string,bool>();
        var items=new List<(string Key,string Method,string Path)>{("equipmentList","post","EquipmentList"),("equipmentDetail","get","EquipmentDetail/{guid}"),("equipmentStats","post","EquipmentStats"),("equipmentCreate","post","EquipmentCreate"),("equipmentEdit","post","EquipmentEdit"),("equipmentService","post","EquipmentService"),("equipmentHold","post","EquipmentHold"),("equipmentRelease","post","EquipmentRelease"),("equipmentArchive","post","EquipmentArchive"),("equipmentTemperature","post","EquipmentTemperature"),("incidents","post","Incidents"),("incidentReport","post","IncidentReport"),("incidentResolve","post","IncidentResolve"),("facility","get","Facility"),("facilitySave","post","FacilitySave"),("stats","post","Stats")};
        foreach(var kind in QualityRules.Kinds){foreach(var action in new[]{"List","Detail","Create","Edit","Line","Submit","Approve"})items.Add((char.ToLowerInvariant(kind[0])+kind[1..]+action,action=="Detail"?"get":"post",kind+action+(action=="Detail"?"/{guid}":"")));if(kind is "Committee" or "Training")items.Add((char.ToLowerInvariant(kind[0])+kind[1..]+"Owners","get",kind+"Owners"));}
        foreach(var action in new[]{"Progress","Verify","Close"})items.Add(("committee"+action,"post","Committee"+action));
        foreach(var x in items)result[x.Key]=await QualityAccess.Can(roles,a,x.Method,"/"+x.Path,ct);
        return new(result,a.UserGuid);
    }
    public async Task<FacilityView> Handle(FacilityQuery cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"get","/Facility",ct);var p=await Db.HemoFacilityProfile.AsNoTracking().SingleOrDefaultAsync(x=>x.OrgLocationGUID==a.OrgLocationGUID,ct);
        if(p==null)return new(await Db.OrgLocation.Where(x=>x.GUID==a.OrgLocationGUID).Select(x=>x.Name).SingleAsync(ct),null,null,null,null,null,null,null,null);
        var history=await Db.HemoFacilityProfileEvent.AsNoTracking().Where(x=>x.HemoFacilityProfileGUID==p.GUID).OrderByDescending(x=>x.OccurredAt).Take(100).Select(x=>new FacilityEventView(x.OccurredAt,x.ActorUserGUID,x.Notes,x.PreviousJson,x.CurrentJson)).ToListAsync(ct);
        return new(p.FacilityName,p.AccreditationCode,p.Chair,p.Secretary,p.Officer,p.BloodCenter,p.BloodCenterCode,p.ReportFooter,p.Version,history);
    }
    public async Task<QualityStats> Handle(QualityStatsQuery cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/Stats",ct);var f=cmd.Filter??new();var org=a.OrgLocationGUID;var now=DateTime.UtcNow;var from=f.From?.UtcDateTime??now.AddDays(-30);var to=f.To?.UtcDateTime??now;
        if(to<from||to-from>TimeSpan.FromDays(3660)||to>now.AddDays(1))throw new ServiceExeption("بازه گزارش معتبر نیست.");
        var q=records.Table.AsNoTracking().Where(x=>x.OrgLocationGUID==org&&x.OccurredAt>=from&&x.OccurredAt<=to);
        if(!string.IsNullOrWhiteSpace(f.Ward))q=q.Where(x=>x.Ward==f.Ward);
        var resolutions=Db.HemoQualityLine.Where(x=>q.Any(r=>r.GUID==x.HemoQualityRecordGUID&&r.Kind=="Committee"&&r.Status=="Approved")&&x.Status!="Verified");
        var attendees=await Db.HemoQualityLine.Where(x=>q.Any(r=>r.GUID==x.HemoQualityRecordGUID&&r.Kind=="Training"&&r.Status=="Approved")).Select(x=>x.DetailsJson).ToListAsync(ct);
        var tx=Db.BloodTransfer.AsNoTracking().Where(x=>x.OrgLocationGUID==org&&x.Unit.OrgLocationGUID==org&&x.Admission.OrgLocationGUID==org);
        var reactions=Db.HemoReaction.Where(x=>x.OrgLocationGUID==org&&x.Transfer.OrgLocationGUID==org&&x.OnsetAt>=from&&x.OnsetAt<=to);
        var incidents=Db.HemoEquipmentIncident.Where(x=>x.Equipment.OrgLocationGUID==org&&x.ReportedAt>=from&&x.ReportedAt<=to);
        if(!string.IsNullOrWhiteSpace(f.Ward)){tx=tx.Where(x=>x.Ward==f.Ward);reactions=reactions.Where(x=>x.Transfer.Ward==f.Ward);incidents=incidents.Where(x=>x.Equipment.Ward==f.Ward);}
        var started=await tx.Where(x=>x.StartedAt>=from&&x.StartedAt<=to).GroupBy(x=>x.ProductCode).Select(g=>new{Key=g.Key,Count=g.Count()}).ToListAsync(ct);
        var completed=tx.Where(x=>x.Status=="Completed"&&x.EndedAt>=from&&x.EndedAt<=to);
        var comp=await completed.GroupBy(x=>x.ProductCode).Select(g=>new{Key=g.Key,Count=g.Count()}).ToListAsync(ct);
        var ward=await completed.GroupBy(x=>x.Ward).Select(g=>new{Key=g.Key,Count=g.Count()}).ToListAsync(ct);
        // Receipts and disposal events are center-level only; no invented ward assignment.
        var bank=Db.BankBloodUnit.Where(x=>x.OrgLocationGUID==org);
        var rec=await bank.Where(x=>x.ReceivedAt>=from&&x.ReceivedAt<=to).GroupBy(x=>x.ProductCode).Select(g=>new{Key=g.Key,Count=g.Count()}).ToListAsync(ct);
        var waste=await Db.BankUnitEvent.Where(x=>x.Unit.OrgLocationGUID==org&&(x.Action=="Discard"||x.Action=="Dispose")&&x.OccurredAt>=from&&x.OccurredAt<=to).GroupBy(x=>x.Unit.ProductCode).Select(g=>new{Key=g.Key,Count=g.Count()}).ToListAsync(ct);
        var products=rec.Select(x=>x.Key).Concat(started.Select(x=>x.Key)).Concat(comp.Select(x=>x.Key)).Concat(waste.Select(x=>x.Key)).Distinct().OrderBy(x=>x).Select(k=>new QualityUsage(k,rec.FirstOrDefault(x=>x.Key==k)?.Count??0,started.FirstOrDefault(x=>x.Key==k)?.Count??0,comp.FirstOrDefault(x=>x.Key==k)?.Count??0,waste.FirstOrDefault(x=>x.Key==k)?.Count??0)).ToList();
        return new(await q.CountAsync(x=>x.Kind=="Qc",ct),await q.CountAsync(x=>x.Kind=="Qc"&&x.Status=="Submitted",ct),await q.CountAsync(x=>x.Kind=="Committee",ct),await resolutions.CountAsync(ct),await resolutions.CountAsync(x=>x.DueAt<now,ct),await q.CountAsync(x=>x.Kind=="Audit",ct),await q.CountAsync(x=>x.Kind=="Training",ct),attendees.Count,attendees.Count(x=>JsonSerializer.Deserialize<QualityLineData>(x)?.Attendance=="Present"),await incidents.CountAsync(ct),await reactions.CountAsync(ct),products,ward.Select(x=>new QualityCount(x.Key,x.Count)).ToList());
    }
}
