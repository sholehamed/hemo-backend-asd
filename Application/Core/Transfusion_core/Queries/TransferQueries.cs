using System.Linq.Expressions;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using Application.Core.PatientAdmission_core;
using Application.Core.BloodBank_core;
namespace Application.Core.Transfusion_core.Queries;
public record TransferListQuery(string? Search=null,string? Status=null,string? Ward=null,Guid? AdmissionGuid=null,Guid? RequestGuid=null,DateTimeOffset? From=null,DateTimeOffset? To=null,int PageNumber=1,int PageSize=20):IRequest<PaginatedList<TransferRow>>;
public record TransferDetailQuery(Guid Guid):IRequest<TransferDetail>;
public record DispatchCandidatesQuery(string? Search=null,Guid? RequestGuid=null,int PageNumber=1,int PageSize=20):IRequest<PaginatedList<DispatchCandidate>>;
public record TransferCapabilitiesQuery:IRequest<TransferCapabilities>;
public record TransferStatsQuery:IRequest<TransferStats>;
public class TransferQueryHandler(BloodTransferService transfers,UserRoleService roles,AuthService auth):
    IRequestHandler<TransferListQuery,PaginatedList<TransferRow>>,IRequestHandler<TransferDetailQuery,TransferDetail>,IRequestHandler<DispatchCandidatesQuery,PaginatedList<DispatchCandidate>>,IRequestHandler<TransferCapabilitiesQuery,TransferCapabilities>,IRequestHandler<TransferStatsQuery,TransferStats>
{
    private IApplicationDbContext Db=>transfers.db;
    private IQueryable<BloodTransfer> Scope(UserRoleInfo u)=>transfers.Table.AsNoTracking().Where(t=>t.OrgLocationGUID==u.OrgLocationGUID&&t.Admission.OrgLocationGUID==u.OrgLocationGUID&&t.Unit.OrgLocationGUID==u.OrgLocationGUID);
    private static Expression<Func<BloodTransfer,TransferRow>> Row()=>t=>new(t.GUID,t.TransferNumber,t.BankBloodUnitGUID,t.BloodRequestGUID,t.PatientAdmissionGUID,t.PatientName,t.HisNumber,t.Ward,t.UnitCode,t.ProductCode,t.BloodGroup,t.Status,t.DispatchedAt,t.ReceivedAt,t.StartedAt,t.EndedAt,t.SuspectedReaction,t.Version);
    public async Task<PaginatedList<TransferRow>> Handle(TransferListQuery f,CancellationToken ct)
    {
        var a=await TransferAccess.Require(roles,auth,"post","/List",ct);var q=Scope(a);
        if(!string.IsNullOrWhiteSpace(f.Search)){var s=f.Search.Trim();q=q.Where(t=>t.TransferNumber.Contains(s)||t.UnitCode.Contains(s)||t.HisNumber.Contains(s)||t.PatientName.Contains(s));}
        if(!string.IsNullOrWhiteSpace(f.Status))q=f.Status=="Active"?q.Where(t=>t.Status=="Dispatched"||t.Status=="Received"||t.Status=="Verified"||t.Status=="Witnessed"||t.Status=="InProgress"):f.Status=="ReturnQueue"?q.Where(t=>t.Status=="Returning"||t.Status=="Returned"||t.Status=="ReceiptHold"||t.Status=="Stopped"):q.Where(t=>t.Status==f.Status);
        if(!string.IsNullOrWhiteSpace(f.Ward))q=q.Where(t=>t.Ward==f.Ward);
        if(f.AdmissionGuid.HasValue)q=q.Where(t=>t.PatientAdmissionGUID==f.AdmissionGuid);
        if(f.RequestGuid.HasValue)q=q.Where(t=>t.BloodRequestGUID==f.RequestGuid);
        if(f.From.HasValue)q=q.Where(t=>t.DispatchedAt>=f.From.Value.UtcDateTime);
        if(f.To.HasValue)q=q.Where(t=>t.DispatchedAt<=f.To.Value.UtcDateTime);
        var page=Math.Clamp(f.PageNumber,1,100000);var size=Math.Clamp(f.PageSize,1,100);var count=await q.CountAsync(ct);
        return new(await q.OrderByDescending(t=>t.DispatchedAt).ThenBy(t=>t.GUID).Skip((page-1)*size).Take(size).Select(Row()).ToListAsync(ct),count,page,size);
    }
    public async Task<PaginatedList<DispatchCandidate>> Handle(DispatchCandidatesQuery f,CancellationToken ct)
    {
        var a=await TransferAccess.Require(roles,auth,"post","/Candidates",ct);var now=DateTime.UtcNow;
        var q=Db.BankBloodUnit.AsNoTracking().Where(u=>u.OrgLocationGUID==a.OrgLocationGUID&&u.Status==BankRules.Ready&&u.ExpiresAt>now&&u.BloodRequestGUID.HasValue&&
            u.BloodRequest!.PatientAdmission.OrgLocationGUID==a.OrgLocationGUID&&u.BloodRequest.Status=="AwaitingBank"&&u.BloodRequest.PatientAdmission.Status=="Active"&&
            u.BloodRequest.PatientAdmission.IdentityStatus=="Verified"&&!u.BloodRequest.PatientAdmission.Patient.IsDelete&&!Db.BloodTransfer.Any(t=>t.BankBloodUnitGUID==u.GUID));
        if(f.RequestGuid.HasValue)q=q.Where(u=>u.BloodRequestGUID==f.RequestGuid);
        if(!string.IsNullOrWhiteSpace(f.Search)){var s=f.Search.Trim();q=q.Where(u=>u.UnitCode.Contains(s)||u.BloodRequest!.RequestNumber.Contains(s)||u.BloodRequest.PatientAdmission.HISNumber.Contains(s));}
        var page=Math.Clamp(f.PageNumber,1,100000);var size=Math.Clamp(f.PageSize,1,100);var count=await q.CountAsync(ct);
        var data=await q.OrderBy(u=>u.ExpiresAt).ThenBy(u=>u.GUID).Skip((page-1)*size).Take(size).Select(u=>new DispatchCandidate(u.GUID,u.Version,u.UnitCode,u.ProductCode,u.BloodGroup,u.ExpiresAt,u.BloodRequestGUID!.Value,u.BloodRequest!.RequestNumber,u.BloodRequest.PatientAdmission.Patient.FirstName+" "+u.BloodRequest.PatientAdmission.Patient.LastName,u.BloodRequest.PatientAdmission.HISNumber,u.BloodRequest.PatientAdmission.Ward,u.BloodRequest.PatientAdmission.WristbandCode)).ToListAsync(ct);
        return new(data,count,page,size);
    }
    public async Task<TransferDetail> Handle(TransferDetailQuery f,CancellationToken ct)
    {
        var a=await TransferAccess.Require(roles,auth,"get","/Detail/{guid}",ct);
        var t=await Scope(a).Include(x=>x.Unit).Include(x=>x.CompatibilityTest).Include(x=>x.BloodRequest).ThenInclude(r=>r.Approvals).Include(x=>x.BloodRequest).ThenInclude(r=>r.PatientAdmission).AsSingleQuery().SingleOrDefaultAsync(x=>x.GUID==f.Guid,ct)??throw new ServiceExeption("حواله یافت نشد.");
        var row=new TransferRow(t.GUID,t.TransferNumber,t.BankBloodUnitGUID,t.BloodRequestGUID,t.PatientAdmissionGUID,t.PatientName,t.HisNumber,t.Ward,t.UnitCode,t.ProductCode,t.BloodGroup,t.Status,t.DispatchedAt,t.ReceivedAt,t.StartedAt,t.EndedAt,t.SuspectedReaction,t.Version);
        var events=await Db.BloodTransferEvent.AsNoTracking().Where(x=>x.BloodTransferGUID==t.GUID).OrderBy(x=>x.OccurredAt).ThenBy(x=>x.GUID).Select(x=>new TransferEventView(x.Action,x.PreviousStatus,x.NewStatus,x.OccurredAt,x.ActorUserGUID,x.ActorRoleGUID,x.Notes,x.EvidenceJson)).ToListAsync(ct);
        var vitals=await Db.BloodTransferVital.AsNoTracking().Where(x=>x.BloodTransferGUID==t.GUID).OrderBy(x=>x.MeasuredAt).ThenBy(x=>x.RecordedAt).ThenBy(x=>x.GUID).Select(x=>new VitalView(x.GUID,x.Phase,x.MeasuredAt,x.RecordedAt,x.Systolic,x.Diastolic,x.Pulse,x.Temperature,x.Spo2,x.Respiration,x.UrineMl,x.Notes,x.ActorUserGUID)).ToListAsync(ct);
        var changed=t.AdmissionVersion==null||!t.AdmissionVersion.SequenceEqual(t.BloodRequest.PatientAdmission.Version);
        return new(row,t.WristbandCode,t.BloodRequest.RequestNumber,t.CompatibilityTest.ReportReference,t.CompatibilityTest.RecordedAt,t.Unit.ExpiresAt,t.CarrierName,t.CarrierCode,t.ContainerCode,t.DispatchTemperature,t.TransportIndicator,t.PackCount,t.VerifiedBy,t.WitnessedBy,t.StartedBy,changed,t.Unit.Status=="Issued"&&TransferRules.Current(t,t.BloodRequest,t.Unit,t.CompatibilityTest,DateTime.UtcNow),t.HisNumber.StartsWith("DEMO-ST05-"),events,vitals);
    }
    public async Task<TransferCapabilities> Handle(TransferCapabilitiesQuery f,CancellationToken ct)
    {
        var u=await AdmissionScope.GetAssignmentAsync(roles,auth,ct);Task<bool> Can(string p,string method="post")=>TransferAccess.Can(roles,u,method,"/"+p,ct);
        return new(await Can("List"),await Can("Detail/{guid}","get"),await Can("Stats"),await Can("Candidates"),await Can("Dispatch"),await Can("Receive"),await Can("Verify"),await Can("Witness"),await Can("Start"),await Can("Vital"),await Can("Complete"),await Can("Stop"),await Can("Return"),await Can("AcceptReturn"),await Can("Dispose"),await Can("DoctorNote"),u.UserGuid);
    }
    public async Task<TransferStats> Handle(TransferStatsQuery f,CancellationToken ct)
    {
        var a=await TransferAccess.Require(roles,auth,"post","/Stats",ct);var q=Scope(a);
        var groups=await q.GroupBy(t=>t.Status).Select(g=>new{Status=g.Key,Count=g.Count()}).ToListAsync(ct);
        int N(string s)=>groups.FirstOrDefault(g=>g.Status==s)?.Count??0;
        return new(groups.Sum(g=>g.Count),N("Dispatched"),N("Received"),N("ReceiptHold"),N("Verified"),N("Witnessed"),N("InProgress"),N("Completed"),N("Stopped"),N("Returning"),N("Returned"),N("Disposed"),await q.CountAsync(t=>t.SuspectedReaction,ct));
    }
}
