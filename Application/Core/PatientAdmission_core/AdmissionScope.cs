using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Core.PatientAdmission_core;

// This is local to the new HEMO module. It follows the existing Api/RoleAccess/MenuApi
// semantics used by ApiService.SetApisToGateway, without changing any shared infrastructure.
internal static class AdmissionScope
{
    public static async Task<UserRoleInfo> GetAssignmentAsync(UserRoleService roles, AuthService auth, CancellationToken ct)
    {
        if (auth.GetUserGUID() == Guid.Empty || auth.GetUserRoleGkey() == 0) throw new ForbiddenAccessException();
        var assigned = await roles.Table.AsNoTracking().AnyAsync(r=>r.Gkey==auth.GetUserRoleGkey() && r.UserGUID==auth.GetUserGUID() && r.ExpireDate>DateTime.Now && !r.User.Blocked && !r.User.ForceChangePassword,ct);
        if(!assigned) throw new ForbiddenAccessException();
        var user = await roles.GetUserRoleInfo();
        if (user.UserGuid != auth.GetUserGUID() || user.ExpireDate <= DateTime.Now) throw new ForbiddenAccessException();
        return user;
    }

    public static async Task<bool> HasAccessAsync(UserRoleService roles, UserRoleInfo user,string method,string path,CancellationToken ct)
    {
        if(user.IsSuperAdmin)return true;
        var apis = roles.db.Api.Where(a => a.Active && a.APITypeName.ToLower() == method.ToLower() &&
            (a.DownstreamPathTemplate.ToLower() == path.ToLower() ||
             a.DownstreamPathTemplate.ToLower() == path.Replace("{guid}", "{guid:guid}").ToLower()));
        return await roles.db.RoleAccess.AnyAsync(a => a.RoleGUID == user.RoleGuid &&
            (a.ApiGUID.HasValue && apis.Any(api => api.GUID == a.ApiGUID.Value) ||
             !a.ApiGUID.HasValue && a.Menu.MenuApis.Any(ma => apis.Any(api => api.GUID == ma.ApiGUID))), ct);
    }

    public static async Task<UserRoleInfo> GetAsync(UserRoleService roles, AuthService auth,string method,string path,CancellationToken ct)
    {
        var user=await GetAssignmentAsync(roles,auth,ct);
        if(!await HasAccessAsync(roles,user,method,path,ct))throw new ForbiddenAccessException();
        return user;
    }
}
