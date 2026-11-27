namespace Application.Core.Hemovigilance_core;
internal static class HvOwnerQuery
{
    public static IQueryable<UserRole> Eligible(IApplicationDbContext db,Guid org)
    {
        var apis=db.Api.Where(x=>x.Active&&x.APITypeName.ToLower()=="post"&&x.DownstreamPathTemplate.ToLower()=="/identity/hemovigilance/capaprogress");
        var now=DateTime.Now;
        return db.UserRole.Where(x=>x.OrgLocationGUID==org&&x.ExpireDate>now&&!x.User.IsDelete&&!x.Role.IsDelete&&
          x.Role.RoleAccess.Any(a=>a.ApiGUID.HasValue&&apis.Any(api=>api.GUID==a.ApiGUID)||!a.ApiGUID.HasValue&&a.Menu.MenuApis.Any(ma=>apis.Any(api=>api.GUID==ma.ApiGUID))));
    }
}
