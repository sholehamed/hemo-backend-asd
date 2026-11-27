using Application.Common.Utility.Security;
namespace Application.Core.PatientAdmission_core.Queries;

public record AdmissionCapabilitiesQuery : IRequest<AdmissionCapabilities>;
public record AdmissionCapabilities(bool Stats,bool List,bool Detail,bool Find,bool History,bool Reviews,bool Create,bool Update,bool Review,bool Identity);

public class AdmissionCapabilitiesQueryHandler(UserRoleService roles,AuthService auth) : IRequestHandler<AdmissionCapabilitiesQuery,AdmissionCapabilities>
{
    public async Task<AdmissionCapabilities> Handle(AdmissionCapabilitiesQuery request,CancellationToken ct)
    {
        var user=await AdmissionScope.GetAssignmentAsync(roles,auth,ct);
        Task<bool> Can(string method,string suffix)=>AdmissionScope.HasAccessAsync(roles,user,method,"/Identity/PatientAdmission"+suffix,ct);
        return new AdmissionCapabilities(await Can("post","/Stats"),await Can("post","/BindPaging"),await Can("get","/{guid}"),
            await Can("get","/FindPatient"),await Can("get","/{guid}/History"),await Can("get","/{guid}/Reviews"),
            await Can("post",""),await Can("put",""),await Can("post","/Review"),await Can("put","/PatientIdentity"));
    }
}
