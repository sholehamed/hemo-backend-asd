using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Core.User_core.Commands.UserLogin;

// Select only active assignments authorized for the HEMO menu, without granting new privileges.
public static class HemoLoginContext
{
    public static IQueryable<UserRole> EligibleRoles(IApplicationDbContext db, Guid userGuid, DateTime now) =>
        db.UserRole.Include(r=>r.Role).Where(r=>r.UserGUID==userGuid && r.ExpireDate>now &&
            (r.Role.Gkey==1 || r.Role.RoleAccess.Any(a=>a.Menu.routerLink=="/admission" || a.Menu.routerLink=="/hemo/requests" || a.Menu.routerLink=="/hemo/bank")));

    public static async Task<Token> IssueAsync(UserService users, User user, CancellationToken ct)
    {
        var roles=await EligibleRoles(users.db,user.GUID,DateTime.Now).ToListAsync(ct);
        if(roles.Count==0)throw new ServiceExeption("نقش معتبر هموویژیلانس برای کاربر تعریف نشده است.");
        var key=await users.CreateToken(user,roles,false);
        return new Token { Key=key.Token, MultiLogin=key.MultiLogin, ExpireDate=key.ExpireDate,
            FirstName=user.FirstName,LastName=user.LastName,ForceChangePassword=user.ForceChangePassword,
            UserRole=roles.OrderBy(r=>r.Role.Level).ThenBy(r=>r.Gkey).First().Gkey };
    }
}
