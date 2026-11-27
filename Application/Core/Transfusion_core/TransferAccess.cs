using Application.Common.Models;
using Application.Common.Exceptions;
using Application.Common.Utility.Security;
using Application.Core.PatientAdmission_core;
namespace Application.Core.Transfusion_core;
internal static class TransferAccess
{
    public const string Base="/Identity/Transfusion";
    public static async Task<bool> Can(UserRoleService roles,UserRoleInfo u,string method,string suffix,CancellationToken ct)
    {
        var path=(Base+suffix).ToLower();var constrained=path.Replace("{guid}","{guid:guid}");
        var apis=roles.db.Api.Where(a=>a.Active&&a.APITypeName.ToLower()==method&&(a.DownstreamPathTemplate.ToLower()==path||a.DownstreamPathTemplate.ToLower()==constrained));
        return await roles.db.RoleAccess.AnyAsync(a=>a.RoleGUID==u.RoleGuid&&
          (a.ApiGUID.HasValue&&apis.Any(api=>api.GUID==a.ApiGUID)||!a.ApiGUID.HasValue&&a.Menu.MenuApis.Any(ma=>apis.Any(api=>api.GUID==ma.ApiGUID))),ct);
    }
    public static async Task<UserRoleInfo> Require(UserRoleService roles,AuthService auth,string method,string suffix,CancellationToken ct)
    {
        var user=await AdmissionScope.GetAssignmentAsync(roles,auth,ct);
        if(!await Can(roles,user,method,suffix,ct))throw new ForbiddenAccessException();
        return user;
    }
}
