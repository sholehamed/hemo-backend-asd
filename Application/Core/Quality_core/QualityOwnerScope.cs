namespace Application.Core.Quality_core;
internal static class QualityOwnerScope
{
    public static IQueryable<UserRole> Eligible(IApplicationDbContext db,Guid org,bool requireProgress)
    {
        var api=db.Api.Where(x=>x.Active&&x.APITypeName.ToLower()=="post"&&x.DownstreamPathTemplate.ToLower()=="/identity/hemoquality/committeeprogress");var now=DateTime.Now;
        return db.UserRole.Where(x=>x.OrgLocationGUID==org&&x.ExpireDate>now&&!x.User.Blocked&&!x.User.ForceChangePassword&&(!requireProgress||x.Role.RoleAccess.Any(a=>a.ApiGUID.HasValue&&api.Any(p=>p.GUID==a.ApiGUID)||!a.ApiGUID.HasValue&&a.Menu.MenuApis.Any(m=>api.Any(p=>p.GUID==m.ApiGUID)))));
    }
}
