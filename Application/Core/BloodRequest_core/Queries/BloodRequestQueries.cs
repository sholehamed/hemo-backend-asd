using System.Linq.Expressions;
using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using Application.Core.PatientAdmission_core;
namespace Application.Core.BloodRequest_core.Queries;

public record BloodRequestListQuery(string? Search=null,string? Status=null,string? Priority=null,string? Ward=null,Guid? PatientAdmissionGuid=null,bool Mine=false,int PageNumber=1,int PageSize=20):IRequest<PaginatedList<RequestRow>>;
public record BloodRequestBankQueueQuery(string? Search=null,string? Priority=null,int PageNumber=1,int PageSize=20):IRequest<PaginatedList<RequestRow>>;
public record BloodRequestDetailQuery(Guid Guid):IRequest<RequestDetail>;
public record BloodRequestBankDetailQuery(Guid Guid):IRequest<RequestDetail>;
public record BloodRequestCapabilitiesQuery:IRequest<RequestCapabilities>;
public record BloodRequestStatsQuery(bool Mine=false):IRequest<RequestStats>;

public class BloodRequestQueryHandler(BloodRequestService requests,UserRoleService roles,AuthService auth):
    IRequestHandler<BloodRequestListQuery,PaginatedList<RequestRow>>,IRequestHandler<BloodRequestBankQueueQuery,PaginatedList<RequestRow>>,
    IRequestHandler<BloodRequestDetailQuery,RequestDetail>,IRequestHandler<BloodRequestBankDetailQuery,RequestDetail>,
    IRequestHandler<BloodRequestCapabilitiesQuery,RequestCapabilities>,IRequestHandler<BloodRequestStatsQuery,RequestStats>
{
    private IQueryable<BloodRequest> Scope(UserRoleInfo u)=>requests.Table.AsNoTracking().Where(r=>r.PatientAdmission.OrgLocationGUID==u.OrgLocationGUID&&!r.PatientAdmission.Patient.IsDelete);
    private static Expression<Func<BloodRequest,RequestRow>> Projection(UserRoleInfo u)=>r=>new(r.GUID,r.RequestNumber,r.PatientAdmissionGUID,r.PatientAdmission.Patient.FirstName+" "+r.PatientAdmission.Patient.LastName,r.PatientAdmission.HISNumber,r.PatientAdmission.Ward,r.Source,r.FormType,r.Priority,r.Status,r.Created,r.SubmittedAt,r.Items.Where(x=>!x.IsDelete).Sum(x=>x.Quantity),r.RequesterUserGUID==u.UserGuid&&r.RequesterRoleGUID==u.RoleGuid);
    private static IQueryable<BloodRequest> Search(IQueryable<BloodRequest> q,string? text)
    {
        if(string.IsNullOrWhiteSpace(text))return q;
        var s=text.Trim();var id=AdmissionRules.NormalizeIdentity(s);
        return q.Where(r=>r.RequestNumber.Contains(id)||r.PatientAdmission.HISNumber.Contains(id)||r.PatientAdmission.Patient.IdentificationNumber.Contains(id)||(r.PatientAdmission.Patient.FirstName+" "+r.PatientAdmission.Patient.LastName).Contains(s));
    }
    private static async Task<PaginatedList<RequestRow>> Page(IQueryable<BloodRequest> q,UserRoleInfo user,int page,int size,CancellationToken ct)
    {
        page=Math.Clamp(page,1,100000);size=Math.Clamp(size,1,100);
        var count=await q.CountAsync(ct);
        var data=await q.OrderByDescending(x=>x.Priority=="Emergency").ThenByDescending(x=>x.Priority=="Urgent").ThenByDescending(x=>x.Created).ThenBy(x=>x.GUID)
            .Skip((page-1)*size).Take(size).Select(Projection(user)).ToListAsync(ct);
        return new(data,count,page,size);
    }
    public async Task<PaginatedList<RequestRow>> Handle(BloodRequestListQuery r,CancellationToken ct)
    {
        var u=await BloodRequestAccess.Require(roles,auth,"post","/BindPaging",false,ct);
        var q=Search(Scope(u),r.Search);
        if(r.Mine)q=q.Where(x=>x.RequesterUserGUID==u.UserGuid&&x.RequesterRoleGUID==u.RoleGuid);
        if(!string.IsNullOrWhiteSpace(r.Status))q=q.Where(x=>x.Status==r.Status);
        if(!string.IsNullOrWhiteSpace(r.Priority))q=q.Where(x=>x.Priority==r.Priority);
        if(!string.IsNullOrWhiteSpace(r.Ward))q=q.Where(x=>x.PatientAdmission.Ward==r.Ward);
        if(r.PatientAdmissionGuid.HasValue)q=q.Where(x=>x.PatientAdmissionGUID==r.PatientAdmissionGuid);
        return await Page(q,u,r.PageNumber,r.PageSize,ct);
    }
    public async Task<PaginatedList<RequestRow>> Handle(BloodRequestBankQueueQuery r,CancellationToken ct)
    {
        var u=await BloodRequestAccess.Require(roles,auth,"post","/BankQueue",false,ct);
        var q=Search(Scope(u).Where(x=>x.Status==BloodRequestWorkflow.AwaitingBank),r.Search);
        if(!string.IsNullOrWhiteSpace(r.Priority))q=q.Where(x=>x.Priority==r.Priority);
        return await Page(q,u,r.PageNumber,r.PageSize,ct);
    }
    private async Task<RequestDetail> Detail(Guid guid,bool bank,CancellationToken ct)
    {
        var u=await BloodRequestAccess.Require(roles,auth,"get",bank?"/Bank/{guid}":"/{guid}",false,ct);
        var q=Scope(u).Where(x=>x.GUID==guid);
        if(bank)q=q.Where(x=>x.Status==BloodRequestWorkflow.AwaitingBank);
        var row=await q.Select(Projection(u)).SingleOrDefaultAsync(ct)??throw new ServiceExeption("درخواست در محدوده مجاز یافت نشد.");
        var r=await q.Include(x=>x.PatientAdmission).Include(x=>x.Items).Include(x=>x.ClinicalData).Include(x=>x.Approvals).AsSplitQuery().SingleAsync(ct);
        var c=r.ClinicalData!;
        var clinical=new ClinicalInput(c.Diagnosis,c.ClinicalIndication,c.Hemoglobin,c.Hematocrit,c.PlateletCount,c.RecentTransfusion,c.RecentPregnancy,c.PriorReaction,c.UnexpectedAntibody,c.Premedication,c.SampleStatus,c.Notes,c.FormDetailsJson==null?null:JsonSerializer.Deserialize<SpecialFormData>(c.FormDetailsJson));
        PhysicianCompletion? Physician(RequestApproval a)
        {
            if(a.Action!="Approve"||a.SnapshotJson==null)return null;
            using var doc=JsonDocument.Parse(a.SnapshotJson);
            return doc.RootElement.TryGetProperty("Physician",out var value)&&value.ValueKind!=JsonValueKind.Null?value.Deserialize<PhysicianCompletion>():null;
        }
        return new(row,r.ClientRequestId,r.Version,r.PatientAdmission.BloodGroup,r.PatientAdmission.IdentityStatus,r.PatientAdmission.BedNumber,
            r.Items.Where(i=>!i.IsDelete).OrderBy(i=>i.ProductCode).Select(i=>new RequestLineInput(i.ProductCode,i.Quantity,i.RequestedBloodGroup,i.SpecialInstructions)).ToList(),clinical,
            r.Approvals.OrderBy(a=>a.OccurredAt).ThenBy(a=>a.GUID).Select(a=>new ApprovalView(a.Action,a.PreviousStatus,a.NewStatus,a.Reason,a.SubmissionAttempt,a.OccurredAt,a.ActorUserGUID,a.ActorRoleGUID,Physician(a))).ToList(),
            r.AdmissionVersionAtSubmission!=null&&!r.AdmissionVersionAtSubmission.SequenceEqual(r.PatientAdmission.Version));
    }
    public Task<RequestDetail> Handle(BloodRequestDetailQuery r,CancellationToken ct)=>Detail(r.Guid,false,ct);
    public Task<RequestDetail> Handle(BloodRequestBankDetailQuery r,CancellationToken ct)=>Detail(r.Guid,true,ct);
    public async Task<RequestCapabilities> Handle(BloodRequestCapabilitiesQuery r,CancellationToken ct)
    {
        var u=await AdmissionScope.GetAssignmentAsync(roles,auth,ct);
        Task<bool> Can(string method,string path,bool explicitGrant=false)=>BloodRequestAccess.Can(roles,u,method,path,explicitGrant,ct);
        return new(await Can("post","/BindPaging"),await Can("get","/{guid}"),await Can("post","/Stats"),await Can("post","/Nurse",true),await Can("post","/Doctor",true),
            await Can("put","",true),await Can("post","/Submit",true),await Can("post","/Cancel",true),await Can("post","/Decide",true),await Can("post","/BankQueue"),await Can("get","/Bank/{guid}"));
    }
    public async Task<RequestStats> Handle(BloodRequestStatsQuery r,CancellationToken ct)
    {
        var u=await BloodRequestAccess.Require(roles,auth,"post","/Stats",false,ct);var q=Scope(u);
        if(r.Mine)q=q.Where(x=>x.RequesterUserGUID==u.UserGuid&&x.RequesterRoleGUID==u.RoleGuid);
        var counts=await q.GroupBy(x=>x.Status).Select(x=>new{Status=x.Key,Count=x.Count()}).ToListAsync(ct);
        int Get(string status)=>counts.FirstOrDefault(x=>x.Status==status)?.Count??0;
        return new(counts.Sum(x=>x.Count),Get("Draft"),Get("AwaitingDoctor"),Get("Returned"),Get("AwaitingBank"),Get("Rejected"),Get("Cancelled"));
    }
}
