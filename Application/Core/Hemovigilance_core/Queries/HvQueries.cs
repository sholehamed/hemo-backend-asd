using System.Linq.Expressions;
using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using Application.Core.PatientAdmission_core;
using Application.Core.Transfusion_core;
namespace Application.Core.Hemovigilance_core.Queries;
public record HvListQuery(HvFilters Filters):IRequest<PaginatedList<HvCaseRow>>;
public record HvDetailQuery(Guid Guid):IRequest<HvDetail>;
public record HvCandidatesQuery(string? Search=null,int PageNumber=1,int PageSize=20):IRequest<PaginatedList<HvCandidate>>;
public record HvStatsQuery(HvFilters Filters):IRequest<HvSummary>;
public record HvOwnersQuery:IRequest<List<HvOwner>>;
public record HvCapabilitiesQuery:IRequest<HvCapabilities>;
public record HvReportsQuery(int PageNumber=1,int PageSize=20):IRequest<PaginatedList<HvReportRow>>;
public record HvReportDetailQuery(Guid Guid):IRequest<HvReportDetail>;
public class HvQueryHandler(HemoReactionService cases,UserRoleService roles,AuthService auth):
    IRequestHandler<HvListQuery,PaginatedList<HvCaseRow>>,IRequestHandler<HvDetailQuery,HvDetail>,IRequestHandler<HvCandidatesQuery,PaginatedList<HvCandidate>>,IRequestHandler<HvStatsQuery,HvSummary>,IRequestHandler<HvOwnersQuery,List<HvOwner>>,IRequestHandler<HvCapabilitiesQuery,HvCapabilities>,IRequestHandler<HvReportsQuery,PaginatedList<HvReportRow>>,IRequestHandler<HvReportDetailQuery,HvReportDetail>
{
    private IApplicationDbContext Db=>cases.db;
    private static Expression<Func<HemoReaction,HvCaseRow>> Row()=>r=>new(r.GUID,r.CaseNumber,r.BloodTransferGUID,r.Transfer.TransferNumber,r.Transfer.PatientName,r.Transfer.HisNumber,r.Transfer.Ward,r.Transfer.ProductCode,r.Transfer.UnitCode,r.Status,r.OnsetAt,r.ReportedAt,r.InitialSeverity,r.DiagnosisCode,r.Severity,r.Imputability,r.Outcome,r.Transfer.HisNumber.StartsWith("DEMO-ST06-")||r.Transfer.HisNumber.StartsWith("DEMO-ST05-"),r.Version);
    private static Expression<Func<HemoReport,HvReportRow>> ReportRow()=>r=>new(r.GUID,r.ReportNumber,r.Title,r.FromAt,r.ToAt,r.Ward,r.ProductCode,r.PreparedAt,r.PreparedBy,r.ExternalStatus,r.ExternalReference,r.Version);
    public async Task<PaginatedList<HvCaseRow>> Handle(HvListQuery f,CancellationToken ct)
    {
        var a=await HvAccess.Require(roles,auth,"post","/List",ct);var filter=f.Filters??new HvFilters();var q=HvReportQuery.Filter(HvReportQuery.Scope(Db,a.OrgLocationGUID),filter);var page=Math.Clamp(filter.PageNumber,1,100000);var size=Math.Clamp(filter.PageSize,1,100);
        return new(await q.OrderByDescending(x=>x.OnsetAt).ThenBy(x=>x.GUID).Skip((page-1)*size).Take(size).Select(Row()).ToListAsync(ct),await q.CountAsync(ct),page,size);
    }
    public async Task<HvDetail> Handle(HvDetailQuery f,CancellationToken ct)
    {
        var a=await HvAccess.Require(roles,auth,"get","/Detail/{guid}",ct);var r=await HvReportQuery.Scope(Db,a.OrgLocationGUID).Include(x=>x.Transfer).SingleOrDefaultAsync(x=>x.GUID==f.Guid,ct)??throw new ServiceExeption("پرونده یافت نشد.");
        var t=r.Transfer;var row=new HvCaseRow(r.GUID,r.CaseNumber,t.GUID,t.TransferNumber,t.PatientName,t.HisNumber,t.Ward,t.ProductCode,t.UnitCode,r.Status,r.OnsetAt,r.ReportedAt,r.InitialSeverity,r.DiagnosisCode,r.Severity,r.Imputability,r.Outcome,t.HisNumber.StartsWith("DEMO-ST06-")||t.HisNumber.StartsWith("DEMO-ST05-"),r.Version);
        var events=await Db.HemoReactionEvent.AsNoTracking().Where(x=>x.HemoReactionGUID==r.GUID).OrderBy(x=>x.OccurredAt).ThenBy(x=>x.GUID).Select(x=>new HvEvent(x.Action,x.PreviousStatus,x.NewStatus,x.OccurredAt,x.ActorUserGUID,x.ActorRoleGUID,x.Notes,x.EvidenceJson)).ToListAsync(ct);
        var actions=await Db.HemoCapa.AsNoTracking().Where(x=>x.HemoReactionGUID==r.GUID).OrderBy(x=>x.DueAt).ThenBy(x=>x.GUID).Select(x=>new CapaView(x.GUID,x.Title,x.Kind,x.OwnerUserGUID,x.OwnerName,x.DueAt,x.Indicator,x.Status,x.Progress,x.Evidence,x.SubmittedAt,x.SubmittedBy,x.VerifiedAt,x.VerifiedBy,x.VerificationNotes,x.Version)).ToListAsync(ct);
        var vitals=await Db.BloodTransferVital.AsNoTracking().Where(x=>x.BloodTransferGUID==t.GUID).OrderBy(x=>x.MeasuredAt).Select(x=>new VitalView(x.GUID,x.Phase,x.MeasuredAt,x.RecordedAt,x.Systolic,x.Diastolic,x.Pulse,x.Temperature,x.Spo2,x.Respiration,x.UrineMl,x.Notes,x.ActorUserGUID,x.RateMlPerHour)).ToListAsync(ct);
        return new(row,t.BloodGroup,t.Status,t.StartedAt,t.EndedAt,t.SuspectedReaction,JsonSerializer.Deserialize<string[]>(r.SymptomsJson)??[],r.InitialNotes,r.ActionsTaken,r.SamplesSent,r.VolumeMl,r.ReporterUserGUID,r.DoctorUserGUID,r.DoctorReviewedAt,r.DoctorNotes,r.DoctorActions,r.DoctorLabRequest,r.FutureRecommendations,r.DoctorReference,r.BankUserGUID,r.BankReviewedAt,r.BankFindings,r.BankSampleReference,r.BankReportReference,r.BankDisposition,r.InvestigationNotes,r.ExternalStatus,r.ExternalReference,r.RcaRequired,r.RcaLead,r.RcaMethod,r.RcaMeetingAt,r.RcaParticipants,r.DirectCause,r.RootCause,r.RcaPublishedAt,r.ClosedAt,events,actions,vitals);
    }
    public async Task<PaginatedList<HvCandidate>> Handle(HvCandidatesQuery f,CancellationToken ct)
    {
        var a=await HvAccess.Require(roles,auth,"post","/Candidates",ct);
        var q=Db.BloodTransfer.AsNoTracking().Where(x=>x.OrgLocationGUID==a.OrgLocationGUID&&x.Admission.OrgLocationGUID==a.OrgLocationGUID&&x.Unit.OrgLocationGUID==a.OrgLocationGUID&&x.StartedAt.HasValue&&!Db.HemoReaction.Any(r=>r.BloodTransferGUID==x.GUID&&r.Status!="Closed"));
        if(!string.IsNullOrWhiteSpace(f.Search)){var s=f.Search.Trim();q=q.Where(x=>x.PatientName.Contains(s)||x.HisNumber.Contains(s)||x.UnitCode.Contains(s)||x.TransferNumber.Contains(s));}
        var page=Math.Clamp(f.PageNumber,1,100000);var size=Math.Clamp(f.PageSize,1,100);
        return new(await q.OrderByDescending(x=>x.SuspectedReaction).ThenByDescending(x=>x.StartedAt).ThenBy(x=>x.GUID).Skip((page-1)*size).Take(size).Select(x=>new HvCandidate(x.GUID,x.TransferNumber,x.PatientName,x.HisNumber,x.Ward,x.ProductCode,x.UnitCode,x.Status,x.StartedAt,x.EndedAt,x.SuspectedReaction,x.Version)).ToListAsync(ct),await q.CountAsync(ct),page,size);
    }
    public async Task<HvSummary> Handle(HvStatsQuery f,CancellationToken ct){var a=await HvAccess.Require(roles,auth,"post","/Stats",ct);return await HvReportQuery.Summary(Db,a.OrgLocationGUID,f.Filters??new HvFilters(),ct);}
    public async Task<List<HvOwner>> Handle(HvOwnersQuery f,CancellationToken ct){var a=await HvAccess.Require(roles,auth,"get","/Owners",ct);return await HvOwnerQuery.Eligible(Db,a.OrgLocationGUID).Select(x=>new HvOwner(x.UserGUID,x.User.FirstName+" "+x.User.LastName)).Distinct().OrderBy(x=>x.Name).ToListAsync(ct);}
    public async Task<HvCapabilities> Handle(HvCapabilitiesQuery f,CancellationToken ct)
    {
        var a=await AdmissionScope.GetAssignmentAsync(roles,auth,ct);Task<bool> Can(string p,string method="post")=>HvAccess.Can(roles,a,method,"/"+p,ct);
        return new(await Can("List"),await Can("Detail/{guid}","get"),await Can("Stats"),await Can("Candidates"),await Can("Report"),await Can("Observe"),await Can("DoctorReview"),await Can("BankReview"),await Can("Investigate"),await Can("ReturnToDoctor"),await Can("PublishRca"),await Can("AssignCapa"),await Can("Owners","get"),await Can("CapaProgress"),await Can("CapaVerify"),await Can("Close"),await Can("External"),await Can("Reports"),await Can("Archive"),await Can("ReportExternal"),a.UserGuid);
    }
    public async Task<PaginatedList<HvReportRow>> Handle(HvReportsQuery f,CancellationToken ct)
    {
        var a=await HvAccess.Require(roles,auth,"post","/Reports",ct);var q=Db.HemoReport.AsNoTracking().Where(x=>x.OrgLocationGUID==a.OrgLocationGUID);var page=Math.Clamp(f.PageNumber,1,100000);var size=Math.Clamp(f.PageSize,1,100);
        return new(await q.OrderByDescending(x=>x.PreparedAt).ThenBy(x=>x.GUID).Skip((page-1)*size).Take(size).Select(ReportRow()).ToListAsync(ct),await q.CountAsync(ct),page,size);
    }
    public async Task<HvReportDetail> Handle(HvReportDetailQuery f,CancellationToken ct)
    {
        var a=await HvAccess.Require(roles,auth,"get","/ReportDetail/{guid}",ct);var r=await Db.HemoReport.AsNoTracking().SingleOrDefaultAsync(x=>x.GUID==f.Guid&&x.OrgLocationGUID==a.OrgLocationGUID,ct)??throw new ServiceExeption("گزارش یافت نشد.");
        return new(new HvReportRow(r.GUID,r.ReportNumber,r.Title,r.FromAt,r.ToAt,r.Ward,r.ProductCode,r.PreparedAt,r.PreparedBy,r.ExternalStatus,r.ExternalReference,r.Version),JsonSerializer.Deserialize<HvSummary>(r.SnapshotJson)??throw new ServiceExeption("محتوای گزارش معتبر نیست."));
    }
}
