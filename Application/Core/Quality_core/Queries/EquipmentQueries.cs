using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using Application.Core.BloodBank_core;
using System.Linq.Expressions;
namespace Application.Core.Quality_core.Queries;
public record EquipmentListQuery(EquipmentFilter Filter):IRequest<PaginatedList<EquipmentRow>>;
public record EquipmentDetailQuery(Guid Guid):IRequest<EquipmentDetail>;
public record EquipmentIncidentsQuery(string? Search=null,string? Status=null,int PageNumber=1,int PageSize=20):IRequest<PaginatedList<IncidentRow>>;
public record EquipmentSummaryQuery:IRequest<EquipmentSummary>;
public class EquipmentQueryHandler(HemoEquipmentService equipment,UserRoleService roles,AuthService auth):
    IRequestHandler<EquipmentListQuery,PaginatedList<EquipmentRow>>,IRequestHandler<EquipmentDetailQuery,EquipmentDetail>,IRequestHandler<EquipmentIncidentsQuery,PaginatedList<IncidentRow>>,IRequestHandler<EquipmentSummaryQuery,EquipmentSummary>
{
    private IApplicationDbContext Db=>equipment.db;
    private static Expression<Func<HemoEquipment,EquipmentRow>> Row()=>e=>new(e.GUID,e.Code,e.Name,e.Category,e.BrandModel,e.Ward,e.SerialNumber,e.Status,e.CalibrationAt,e.CalibrationDueAt,e.MaintenanceDueAt,e.Version);
    private static Expression<Func<HemoEquipmentIncident,IncidentRow>> Incident()=>i=>new(i.GUID,i.HemoEquipmentGUID,i.Equipment.Code,i.Equipment.Name,i.IncidentNumber,i.Urgency,i.Status,i.ReportedAt,i.Description,i.ReporterUserGUID,i.Resolution,i.ResolutionReference,i.ResolvedAt,i.Version,i.Equipment.Version);
    private IQueryable<HemoEquipment> Scope(Guid org)=>equipment.Table.AsNoTracking().Where(x=>x.OrgLocationGUID==org);
    public async Task<PaginatedList<EquipmentRow>> Handle(EquipmentListQuery cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/EquipmentList",ct);var f=cmd.Filter??new();var q=Scope(a.OrgLocationGUID);
        if(!string.IsNullOrWhiteSpace(f.Search)){var s=f.Search.Trim();q=q.Where(x=>x.Code.Contains(s)||x.Name.Contains(s)||x.SerialNumber!=null&&x.SerialNumber.Contains(s));}
        if(!string.IsNullOrWhiteSpace(f.Category))q=q.Where(x=>x.Category==f.Category);
        if(!string.IsNullOrWhiteSpace(f.Ward))q=q.Where(x=>x.Ward==f.Ward);
        var now=DateTime.UtcNow;if(!string.IsNullOrWhiteSpace(f.Status))q=f.Status=="Due"?q.Where(x=>x.Status!="Archived"&&(x.CalibrationDueAt<=now||x.MaintenanceDueAt<=now)):q.Where(x=>x.Status==f.Status);
        var p=Math.Clamp(f.PageNumber,1,100000);var size=Math.Clamp(f.PageSize,1,100);
        return new(await q.OrderBy(x=>x.Code).Skip((p-1)*size).Take(size).Select(Row()).ToListAsync(ct),await q.CountAsync(ct),p,size);
    }
    public async Task<EquipmentDetail> Handle(EquipmentDetailQuery cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"get","/EquipmentDetail/{guid}",ct);var e=await Scope(a.OrgLocationGUID).Where(x=>x.GUID==cmd.Guid).Select(Row()).SingleOrDefaultAsync(ct)??throw new ServiceExeption("تجهیز یافت نشد.");
        var events=await Db.HemoEquipmentEvent.AsNoTracking().Where(x=>x.HemoEquipmentGUID==e.Guid).OrderByDescending(x=>x.OccurredAt).Select(x=>new EquipmentEventRow(x.Kind,x.PreviousStatus,x.NewStatus,x.OccurredAt,x.ActorUserGUID,x.Notes,x.Reference,x.Provider,x.DueAt,x.EvidenceJson)).ToListAsync(ct);
        var incidents=await Db.HemoEquipmentIncident.AsNoTracking().Where(x=>x.HemoEquipmentGUID==e.Guid).OrderByDescending(x=>x.ReportedAt).Select(Incident()).ToListAsync(ct);
        var temp=await Db.BankTemperatureLog.AsNoTracking().Where(x=>x.OrgLocationGUID==a.OrgLocationGUID&&x.EquipmentCode==e.Code).OrderByDescending(x=>x.MeasuredAt).ThenByDescending(x=>x.RecordedAt).Take(100).Select(x=>new TemperatureView(x.GUID,x.EquipmentCode,x.MeasuredAt,x.RecordedAt,x.Temperature,x.Assessment,x.Notes,x.ActorUserGUID)).ToListAsync(ct);
        var units=await Db.BankBloodUnit.CountAsync(x=>x.OrgLocationGUID==a.OrgLocationGUID&&x.EquipmentCode==e.Code&&(x.Status=="ReceivedHold"||x.Status=="Available"||x.Status=="Reserved"||x.Status=="Thawing"||x.Status=="Ready"||x.Status=="Quarantined"),ct);
        return new(e,events,incidents,temp,units,e.Code.StartsWith("DEMO-"));
    }
    public async Task<PaginatedList<IncidentRow>> Handle(EquipmentIncidentsQuery f,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/Incidents",ct);var q=Db.HemoEquipmentIncident.AsNoTracking().Where(x=>x.Equipment.OrgLocationGUID==a.OrgLocationGUID);
        if(!string.IsNullOrWhiteSpace(f.Search)){var s=f.Search.Trim();q=q.Where(x=>x.IncidentNumber.Contains(s)||x.Equipment.Code.Contains(s)||x.Equipment.Name.Contains(s));}
        if(!string.IsNullOrWhiteSpace(f.Status))q=q.Where(x=>x.Status==f.Status);
        var p=Math.Clamp(f.PageNumber,1,100000);var size=Math.Clamp(f.PageSize,1,100);return new(await q.OrderByDescending(x=>x.ReportedAt).ThenBy(x=>x.GUID).Skip((p-1)*size).Take(size).Select(Incident()).ToListAsync(ct),await q.CountAsync(ct),p,size);
    }
    public async Task<EquipmentSummary> Handle(EquipmentSummaryQuery cmd,CancellationToken ct)
    {
        var a=await QualityAccess.Require(roles,auth,"post","/EquipmentStats",ct);var q=Scope(a.OrgLocationGUID);var now=DateTime.UtcNow;
        var legacy=await Db.BankBloodUnit.Where(x=>x.OrgLocationGUID==a.OrgLocationGUID&&!q.Any(e=>e.Code==x.EquipmentCode)).Select(x=>x.EquipmentCode).Union(Db.BankTemperatureLog.Where(x=>x.OrgLocationGUID==a.OrgLocationGUID&&!q.Any(e=>e.Code==x.EquipmentCode)).Select(x=>x.EquipmentCode)).OrderBy(x=>x).ToListAsync(ct);
        return new(await q.CountAsync(ct),await q.CountAsync(x=>x.Status=="Operational"&&(!x.CalibrationDueAt.HasValue||x.CalibrationDueAt>now)&&(!x.MaintenanceDueAt.HasValue||x.MaintenanceDueAt>now),ct),await q.CountAsync(x=>x.Status=="Hold",ct),await q.CountAsync(x=>x.Status=="Archived",ct),await q.CountAsync(x=>x.Status!="Archived"&&(x.CalibrationDueAt<=now||x.MaintenanceDueAt<=now),ct),await Db.HemoEquipmentIncident.CountAsync(x=>x.Equipment.OrgLocationGUID==a.OrgLocationGUID&&x.Status=="Open",ct),legacy.Count,legacy.Take(100).ToList());
    }
}
