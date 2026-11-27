using Application.Common.Models;

namespace Application.Core.RoleAccess_core.Commands.RoleAccessUpdateAccessFromOrglocation;

public record RoleAccessUpdateAccessFromOrglocationCommand : IRequest, IMapFrom<RoleAccess>
{
    [PropConfig("لیست منو")] public List<Guid> MenuGUIDs { get; init; }

    [PropConfig("نقش")] public Guid RoleGUID { get; init; }

    [PropConfig("دریافت شده از پکیج")] public Guid? OrglocationPackageGUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<RoleAccessUpdateAccessFromOrglocationCommand, RoleAccess>();
    }
}

public class
    RoleAccessUpdateAccessFromOrglocationCommandHandler : IRequestHandler<RoleAccessUpdateAccessFromOrglocationCommand>
{
    private RoleAccessService RoleAccessService;
    private OrglocationPackageService OrglocationPackageService;
    private UserRoleService UserRoleService;
    private RoleService RoleService;
    private ApiService ApiService;

    public RoleAccessUpdateAccessFromOrglocationCommandHandler(RoleAccessService roleAccessService, OrglocationPackageService orglocationPackageService, UserRoleService userRoleService, RoleService roleService, ApiService apiService)
    {
        RoleAccessService = roleAccessService;
        OrglocationPackageService = orglocationPackageService;
        UserRoleService = userRoleService;
        RoleService = roleService;
        ApiService = apiService;
    }

    public async Task Handle(RoleAccessUpdateAccessFromOrglocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await RoleService.RoleIsValid(request.RoleGUID);

            UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();

            OrglocationPackage orglocationPackage =
                await OrglocationPackageService.GetValidOrglocationPackage((Guid)request.OrglocationPackageGUID,
                    userRoleInfo.OrgLocationGUID);

            List<RoleAccess> OLdAccess = RoleAccessService.db.RoleAccess.IgnoreQueryFilters().Where(a =>
                a.OrglocationPackageGUID == request.OrglocationPackageGUID &&
                a.ApiGUID == null &&
                a.RoleGUID == request.RoleGUID
            ).ToList();

            List<RoleAccess> RemoveList = OLdAccess.Where(a => request.MenuGUIDs.All(b => b != a.MenuGUID)).ToList();

        

            foreach (var MenuGUID in request.MenuGUIDs)
            {
                RoleAccess? currentRoleAccess = RoleAccessService.Table.FirstOrDefault(a =>
                    a.ApiGUID == null &&
                    a.MenuGUID == MenuGUID &&
                    a.RoleGUID == orglocationPackage.Package.RoleGUID
                );
                if (currentRoleAccess != null)
                {
                    RoleAccess? OldRoleAccess = OLdAccess.FirstOrDefault(a =>
                        a.ApiGUID == null &&
                        a.RoleGUID == request.RoleGUID &&
                        a.MenuGUID == MenuGUID
                    );
                    if (OldRoleAccess == null)
                    {
                        RoleAccessService.Table.Add(
                            new RoleAccess()
                            {
                                MenuGUID = MenuGUID,
                                RoleGUID = request.RoleGUID,
                                ApiGUID = null,
                                PRoleAccessGUID = currentRoleAccess.GUID,
                                OrglocationPackageGUID = request.OrglocationPackageGUID,
                            }
                        );
                        await RoleAccessService.SaveChangesAsync(cancellationToken);
                    }
                    else if (OldRoleAccess.IsDelete)
                    {
                        OldRoleAccess.IsDelete = false;
                        OldRoleAccess.Deleted = null;
                        OldRoleAccess.DeletedBy = null;
                        OldRoleAccess.DeletedByIP = null;
                    }
                }
            }
            RoleAccessService.Table.RemoveRange(RemoveList);
            await RoleAccessService.SaveChangesAsync(cancellationToken);
            ApiService.UpateApiListInBackground();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}