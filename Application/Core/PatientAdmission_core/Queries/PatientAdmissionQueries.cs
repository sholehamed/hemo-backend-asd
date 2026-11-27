using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Core.PatientAdmission_core.Queries;

public record PatientAdmissionWithPaginationQuery(string? Search=null,string? Ward=null,string? IdentityStatus=null,string? BloodGroup=null,string? Status=null,int PageNumber=1,int PageSize=20,DateTime? From=null,DateTime? To=null) : IRequest<PaginatedList<AdmissionView>>;
public record PatientAdmissionFindByIdQuery(Guid Guid) : IRequest<AdmissionView>;
public record PatientFindByIdentityQuery(string IdentificationType,string IdentificationNumber) : IRequest<PatientIdentityView?>;
public record PatientAdmissionHistoryQuery(Guid Guid) : IRequest<List<AdmissionView>>;
public record PatientAdmissionReviewsQuery(Guid Guid) : IRequest<List<AdmissionReviewView>>;
public record PatientAdmissionStatsQuery(DateTime? From=null,DateTime? To=null,string? Ward=null) : IRequest<AdmissionStats>;

public class PatientAdmissionQueryHandler(PatientAdmissionService admissions,PatientService patients,PatientAdmissionReviewService reviews,UserRoleService roles,AuthService auth) : IRequestHandler<PatientAdmissionWithPaginationQuery,PaginatedList<AdmissionView>>,IRequestHandler<PatientAdmissionFindByIdQuery,AdmissionView>,IRequestHandler<PatientFindByIdentityQuery,PatientIdentityView?>,IRequestHandler<PatientAdmissionHistoryQuery,List<AdmissionView>>,IRequestHandler<PatientAdmissionReviewsQuery,List<AdmissionReviewView>>,IRequestHandler<PatientAdmissionStatsQuery,AdmissionStats>
{
    private async Task<IQueryable<PatientAdmission>> Scope(string method,string path,CancellationToken ct)
    {
        var user=await AdmissionScope.GetAsync(roles,auth,method,path,ct);
        return admissions.Table.AsNoTracking().Where(a=>a.OrgLocationGUID==user.OrgLocationGUID && !a.Patient.IsDelete);
    }
    public async Task<PaginatedList<AdmissionView>> Handle(PatientAdmissionWithPaginationQuery r,CancellationToken ct)
    {
        var q=await Scope("post","/Identity/PatientAdmission/BindPaging",ct);
        if(!string.IsNullOrWhiteSpace(r.Search)){var s=r.Search.Trim();var id=AdmissionRules.NormalizeIdentity(s);q=q.Where(a=>a.HISNumber.Contains(id)||a.WristbandCode==id||a.Patient.IdentificationNumber.Contains(id)||(a.Patient.FirstName+" "+a.Patient.LastName).Contains(s));}
        if(!string.IsNullOrWhiteSpace(r.Ward))q=q.Where(a=>a.Ward==r.Ward);
        if(!string.IsNullOrWhiteSpace(r.IdentityStatus))q=q.Where(a=>a.IdentityStatus==r.IdentityStatus);
        if(!string.IsNullOrWhiteSpace(r.BloodGroup))q=q.Where(a=>a.BloodGroup==r.BloodGroup);
        if(!string.IsNullOrWhiteSpace(r.Status))q=q.Where(a=>a.Status==r.Status);
        if(r.From.HasValue)q=q.Where(a=>a.AdmittedAt>=r.From.Value);
        if(r.To.HasValue)q=q.Where(a=>a.AdmittedAt<r.To.Value.Date.AddDays(1));
        var page=Math.Clamp(r.PageNumber,1,100000);var size=Math.Clamp(r.PageSize,1,100);
        var count=await q.CountAsync(ct);
        var data=await q.OrderByDescending(a=>a.AdmittedAt).ThenBy(a=>a.GUID).Skip((page-1)*size).Take(size).Select(AdmissionProjection.View).ToListAsync(ct);
        return new PaginatedList<AdmissionView>(data,count,page,size);
    }
    public async Task<AdmissionView> Handle(PatientAdmissionFindByIdQuery r,CancellationToken ct)
    {
        var q=await Scope("get","/Identity/PatientAdmission/{guid}",ct);
        return await q.Where(a=>a.GUID==r.Guid).Select(AdmissionProjection.View).SingleOrDefaultAsync(ct)??throw new ServiceExeption("پرونده در محدوده مجاز یافت نشد.");
    }
    public async Task<PatientIdentityView?> Handle(PatientFindByIdentityQuery r,CancellationToken ct)
    {
        var user=await AdmissionScope.GetAsync(roles,auth,"get","/Identity/PatientAdmission/FindPatient",ct);var id=AdmissionRules.NormalizeIdentity(r.IdentificationNumber);
        return await patients.Table.AsNoTracking().Where(p=>p.OrgLocationGUID==user.OrgLocationGUID&&p.IdentificationType==r.IdentificationType&&p.IdentificationNumber==id).Select(p=>new PatientIdentityView(p.GUID,p.FirstName,p.LastName,p.FatherName,p.BirthDate,p.Gender,p.Mobile)).SingleOrDefaultAsync(ct);
    }
    public async Task<List<AdmissionView>> Handle(PatientAdmissionHistoryQuery r,CancellationToken ct)
    {
        var q=await Scope("get","/Identity/PatientAdmission/{guid}/History",ct);
        var patientGuid=await q.Where(x=>x.GUID==r.Guid).Select(x=>(Guid?)x.PatientGUID).SingleOrDefaultAsync(ct)??throw new ServiceExeption("پرونده در محدوده مجاز یافت نشد.");
        return await q.Where(x=>x.PatientGUID==patientGuid).OrderByDescending(x=>x.AdmittedAt).Select(AdmissionProjection.View).ToListAsync(ct);
    }
    public async Task<List<AdmissionReviewView>> Handle(PatientAdmissionReviewsQuery r,CancellationToken ct)
    {
        var q=await Scope("get","/Identity/PatientAdmission/{guid}/Reviews",ct);
        if(!await q.AnyAsync(x=>x.GUID==r.Guid,ct))throw new ServiceExeption("پرونده در محدوده مجاز یافت نشد.");
        return await reviews.Table.AsNoTracking().Where(x=>x.PatientAdmissionGUID==r.Guid).OrderByDescending(x=>x.ReviewedAt).Select(x=>new AdmissionReviewView(x.Kind,x.Outcome,x.Checklist,x.Notes,x.ReviewedAt)).ToListAsync(ct);
    }
    public async Task<AdmissionStats> Handle(PatientAdmissionStatsQuery r,CancellationToken ct)
    {
        var q=await Scope("post","/Identity/PatientAdmission/Stats",ct);
        if(r.From.HasValue)q=q.Where(a=>a.AdmittedAt>=r.From.Value);
        if(r.To.HasValue)q=q.Where(a=>a.AdmittedAt<r.To.Value.Date.AddDays(1));
        if(!string.IsNullOrWhiteSpace(r.Ward))q=q.Where(a=>a.Ward==r.Ward);
        return new AdmissionStats(await q.CountAsync(ct),await q.CountAsync(a=>a.Status=="Active",ct),await q.CountAsync(a=>a.IdentityStatus=="Verified",ct),await q.CountAsync(a=>a.IdentityStatus=="Mismatch",ct),await q.CountAsync(a=>a.Status=="ReadyForDischarge",ct),await q.GroupBy(a=>a.Ward).Select(g=>new AdmissionGroup(g.Key,g.Count())).ToListAsync(ct),await q.GroupBy(a=>a.BloodGroup??"Unknown").Select(g=>new AdmissionGroup(g.Key,g.Count())).ToListAsync(ct));
    }
}
