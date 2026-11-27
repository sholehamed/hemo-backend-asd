using System.Linq.Expressions;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using Application.Core.PatientAdmission_core;
namespace Application.Core.BloodBank_core.Queries;
public record BankInventoryQuery(string? Search=null,string? Product=null,string? Group=null,string? Status=null,Guid? RequestGuid=null,string? Equipment=null,int PageNumber=1,int PageSize=20):IRequest<PaginatedList<UnitRow>>;
public record BankUnitQuery(Guid Guid):IRequest<UnitDetail>;
public record BankTemperatureQuery(string? Equipment=null,string? Assessment=null,DateTimeOffset? From=null,DateTimeOffset? To=null,int PageNumber=1,int PageSize=20):IRequest<PaginatedList<TemperatureView>>;
public record BankCapabilitiesQuery:IRequest<BankCapabilities>;
public record BankStatsQuery:IRequest<BankStats>;

public class BankQueryHandler(BankBloodUnitService units,UserRoleService roles,AuthService auth):
    IRequestHandler<BankInventoryQuery,PaginatedList<UnitRow>>,IRequestHandler<BankUnitQuery,UnitDetail>,IRequestHandler<BankTemperatureQuery,PaginatedList<TemperatureView>>,IRequestHandler<BankCapabilitiesQuery,BankCapabilities>,IRequestHandler<BankStatsQuery,BankStats>
{
    private IApplicationDbContext Db=>units.db;
    private IQueryable<BankBloodUnit> Scope(UserRoleInfo u)=>units.Table.AsNoTracking().Where(x=>x.OrgLocationGUID==u.OrgLocationGUID);
    private static Expression<Func<BankBloodUnit,UnitRow>> Row(DateTime now)=>x=>new UnitRow(x.GUID,x.UnitCode,x.ProductCode,x.BloodGroup,x.BatchNumber,x.SupplySource,x.EquipmentCode,x.ReceivedAt,x.ExpiresAt,x.Status,x.ExpiresAt<=now,x.BloodRequestGUID,x.ThawStartedAt,x.ThawCompletedAt,x.Version);
    private static Expression<Func<BankTemperatureLog,TemperatureView>> Temp()=>t=>new TemperatureView(t.GUID,t.EquipmentCode,t.MeasuredAt,t.RecordedAt,t.Temperature,t.Assessment,t.Notes,t.ActorUserGUID);
    public async Task<PaginatedList<UnitRow>> Handle(BankInventoryQuery f,CancellationToken ct)
    {
        var u=await BankAccess.Require(roles,auth,"post","/Inventory",ct);var q=Scope(u);var now=DateTime.UtcNow;
        if(!string.IsNullOrWhiteSpace(f.Search)){var s=BankValidation.Normalize(f.Search);q=q.Where(x=>x.UnitCode.Contains(s)||x.BatchNumber!.Contains(s)||x.SupplySource.Contains(f.Search));}
        if(!string.IsNullOrEmpty(f.Product))q=q.Where(x=>x.ProductCode==f.Product);
        if(!string.IsNullOrEmpty(f.Group))q=q.Where(x=>x.BloodGroup==f.Group);
        if(!string.IsNullOrEmpty(f.Status))q=f.Status=="Expired"?q.Where(x=>x.ExpiresAt<=now&&x.Status!=BankRules.Discarded):f.Status=="ExpiringSoon"?q.Where(x=>x.ExpiresAt>now&&x.ExpiresAt<=now.AddDays(3)&&x.Status!=BankRules.Discarded):q.Where(x=>x.Status==f.Status&&(f.Status!=BankRules.Available||x.ExpiresAt>now));
        if(f.RequestGuid.HasValue)q=q.Where(x=>x.BloodRequestGUID==f.RequestGuid.Value);
        if(!string.IsNullOrWhiteSpace(f.Equipment)){var e=BankValidation.Normalize(f.Equipment);q=q.Where(x=>x.EquipmentCode==e);}
        var page=Math.Clamp(f.PageNumber,1,100000);var size=Math.Clamp(f.PageSize,1,100);var count=await q.CountAsync(ct);
        return new(await q.OrderBy(x=>x.ExpiresAt).ThenBy(x=>x.GUID).Skip((page-1)*size).Take(size).Select(Row(now)).ToListAsync(ct),count,page,size);
    }
    public async Task<UnitDetail> Handle(BankUnitQuery f,CancellationToken ct)
    {
        var u=await BankAccess.Require(roles,auth,"get","/Unit/{guid}",ct);
        var unit=await Scope(u).Where(x=>x.GUID==f.Guid).Select(Row(DateTime.UtcNow)).SingleOrDefaultAsync(ct)??throw new ServiceExeption("واحد یافت نشد.");
        var events=await Db.BankUnitEvent.AsNoTracking().Where(x=>x.BankBloodUnitGUID==unit.Guid).OrderByDescending(x=>x.OccurredAt).ThenByDescending(x=>x.GUID).Select(x=>new UnitEventView(x.Action,x.PreviousStatus,x.NewStatus,x.BloodRequestGUID,x.OccurredAt,x.ActorUserGUID,x.ActorRoleGUID,x.Reason,x.Method,x.Temperature,x.EffectiveExpiry)).ToListAsync(ct);
        var tests=await Db.BankCompatibilityTest.AsNoTracking().Where(x=>x.BankBloodUnitGUID==unit.Guid).OrderByDescending(x=>x.RecordedAt).ThenByDescending(x=>x.GUID).Select(x=>new LabView(x.GUID,x.BloodRequestGUID,x.SubmissionAttempt,x.RecordedAt,x.ActorUserGUID,x.SampleReference,x.SecondSample,x.LabelGroup,x.Method,x.ImmediateSpin,x.Phase37,x.Ahg,x.Dat,x.AntibodyScreen,x.ReportReference,x.Conclusion,x.Notes)).ToListAsync(ct);
        var latest=await Db.BankTemperatureLog.AsNoTracking().Where(t=>t.OrgLocationGUID==u.OrgLocationGUID&&t.EquipmentCode==unit.EquipmentCode).OrderByDescending(t=>t.MeasuredAt).ThenByDescending(t=>t.RecordedAt).ThenByDescending(t=>t.GUID).Select(Temp()).FirstOrDefaultAsync(ct);
        return new(unit,events,tests,latest);
    }
    public async Task<PaginatedList<TemperatureView>> Handle(BankTemperatureQuery f,CancellationToken ct)
    {
        var u=await BankAccess.Require(roles,auth,"post","/TemperatureHistory",ct);var q=Db.BankTemperatureLog.AsNoTracking().Where(t=>t.OrgLocationGUID==u.OrgLocationGUID);
        if(!string.IsNullOrWhiteSpace(f.Equipment)){var e=BankValidation.Normalize(f.Equipment);q=q.Where(t=>t.EquipmentCode==e);}
        if(!string.IsNullOrWhiteSpace(f.Assessment))q=q.Where(t=>t.Assessment==f.Assessment);
        if(f.From.HasValue)q=q.Where(t=>t.MeasuredAt>=f.From.Value.UtcDateTime);
        if(f.To.HasValue)q=q.Where(t=>t.MeasuredAt<=f.To.Value.UtcDateTime);
        var page=Math.Clamp(f.PageNumber,1,100000);var size=Math.Clamp(f.PageSize,1,100);var count=await q.CountAsync(ct);
        return new(await q.OrderByDescending(t=>t.MeasuredAt).ThenByDescending(t=>t.RecordedAt).ThenByDescending(t=>t.GUID).Skip((page-1)*size).Take(size).Select(Temp()).ToListAsync(ct),count,page,size);
    }
    public async Task<BankCapabilities> Handle(BankCapabilitiesQuery f,CancellationToken ct)
    {
        var u=await AdmissionScope.GetAssignmentAsync(roles,auth,ct);
        Task<bool> Can(string p,string method="post")=>BankAccess.Can(roles,u,method,"/"+p,ct);
        return new(await Can("Inventory"),await Can("Unit/{guid}","get"),await Can("Stats"),await Can("TemperatureHistory"),await Can("Receive"),await Can("Inspect"),await Can("Test"),await Can("Reserve"),await Can("Thaw"),await Can("Ready"),await Can("Hold"),await Can("Unreserve"),await Can("Discard"),await Can("Temperature"),await Can("ReturnRequest"));
    }
    public async Task<BankStats> Handle(BankStatsQuery f,CancellationToken ct)
    {
        var u=await BankAccess.Require(roles,auth,"post","/Stats",ct);var q=Scope(u);var now=DateTime.UtcNow;
        var data=await q.Select(x=>new{x.Status,x.ExpiresAt}).ToListAsync(ct);
        int Count(string s)=>data.Count(x=>x.Status==s&&x.ExpiresAt>now);
        var approvals=await Db.BloodRequest.CountAsync(r=>r.PatientAdmission.OrgLocationGUID==u.OrgLocationGUID&&r.Status=="AwaitingBank",ct);
        var concern=await Db.BankTemperatureLog.Where(t=>t.OrgLocationGUID==u.OrgLocationGUID&&t.Assessment!="Acceptable"&&t.MeasuredAt>=now.AddDays(-1)).CountAsync(ct);
        return new(data.Count,Count(BankRules.Available),Count(BankRules.Reserved),Count(BankRules.Thawing),Count(BankRules.Ready),Count(BankRules.ReceivedHold)+Count(BankRules.Quarantined),data.Count(x=>x.Status!=BankRules.Discarded&&x.ExpiresAt<=now),data.Count(x=>x.Status!=BankRules.Discarded&&x.ExpiresAt>now&&x.ExpiresAt<=now.AddDays(3)),data.Count(x=>x.Status==BankRules.Discarded),approvals,concern);
    }
}
