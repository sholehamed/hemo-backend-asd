using Application.Common.Models;
using Application.Common.Exceptions;
using Application.Common.Utility.Security;
using Application.Core.PatientAdmission_core;
namespace Application.Core.BloodRequest_core;

internal static class BloodRequestAccess
{
    public const string Base="/Identity/BloodRequest";
    // Clinical actions must be granted explicitly, even for a system administrator.
    public static async Task<bool> Can(UserRoleService roles,UserRoleInfo user,string method,string suffix,bool explicitGrant,CancellationToken ct)
    {
        if(!explicitGrant)return await AdmissionScope.HasAccessAsync(roles,user,method,Base+suffix,ct);
        var path=(Base+suffix).ToLower();
        return await roles.db.RoleAccess.AnyAsync(a=>a.RoleGUID==user.RoleGuid&&
            (a.ApiGUID.HasValue&&roles.db.Api.Any(api=>api.GUID==a.ApiGUID&&api.Active&&api.APITypeName.ToLower()==method&&api.DownstreamPathTemplate.ToLower()==path)
            ||!a.ApiGUID.HasValue&&a.Menu.MenuApis.Any(ma=>ma.Api.Active&&ma.Api.APITypeName.ToLower()==method&&ma.Api.DownstreamPathTemplate.ToLower()==path)),ct);
    }
    public static async Task<UserRoleInfo> Require(UserRoleService roles,AuthService auth,string method,string suffix,bool explicitGrant,CancellationToken ct)
    {
        var user=await AdmissionScope.GetAssignmentAsync(roles,auth,ct);
        if(!await Can(roles,user,method,suffix,explicitGrant,ct))throw new ForbiddenAccessException();
        return user;
    }
}
