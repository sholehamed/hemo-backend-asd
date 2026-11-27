using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Core.PatientAdmission_core;

internal static class AdmissionScope
{
    public static async Task<UserRoleInfo> GetAsync(UserRoleService roles, AuthService auth, CancellationToken ct)
    {
        if (auth.GetUserGUID() == Guid.Empty || auth.GetUserRoleGkey() == 0) throw new ForbiddenAccessException();
        var user = await roles.GetUserRoleInfo();
        if (user.UserGuid != auth.GetUserGUID() || user.ExpireDate <= DateTime.Now) throw new ForbiddenAccessException();
        if (!user.IsSuperAdmin && !await roles.db.RoleAccess.AnyAsync(a => a.RoleGUID == user.RoleGuid && a.Menu.routerLink == "/admission", ct))
            throw new ForbiddenAccessException();
        return user;
    }
}
