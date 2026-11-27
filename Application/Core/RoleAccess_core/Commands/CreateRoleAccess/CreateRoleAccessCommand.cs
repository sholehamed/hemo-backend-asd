using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Core.RoleAccess_core.Commands.CreateRoleAccess;

public record CreateRoleAccessCommand : IRequest<Guid>, IMapFrom<RoleAccess>
{
    [PropConfig("منو")] public Guid MenuGUID { get; init; }

    [PropConfig("نقش")] public Guid RoleGUID { get; init; }

    [PropConfig("سرویس")] public List<Guid?> ApiGUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateRoleAccessCommand, RoleAccess>();
    }
}

public class CreateRoleAccessCommandHandler : IRequestHandler<CreateRoleAccessCommand, Guid>
{
    private RoleService RoleService;
    private RoleAccessService RoleAccessService;
    private ApiService ApiService;
    private UserRoleService UserRoleService;

    public CreateRoleAccessCommandHandler(RoleService roleService, RoleAccessService roleAccessService,
        ApiService apiService, UserRoleService userRoleService)
    {
        RoleService = roleService;
        RoleAccessService = roleAccessService;
        ApiService = apiService;
        UserRoleService = userRoleService;
    }

    public async Task<Guid> Handle(CreateRoleAccessCommand request, CancellationToken cancellationToken)
    {
        await RoleService.RoleIsValid(request.RoleGUID);
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();

        foreach (var ApiGUID in request.ApiGUID)
        {
            RoleAccess? PRoleAccess = null;
            if (!userRoleInfo.IsSuperAdmin)
            {
                PRoleAccess =
                    RoleAccessService.Table.FirstOrDefault(a =>
                        a.MenuGUID == request.MenuGUID && a.ApiGUID == ApiGUID && a.RoleGUID == userRoleInfo.RoleGuid);

                if (PRoleAccess == null)
                {
                    PRoleAccess = RoleAccessService.Table.FirstOrDefault(a =>
                        a.MenuGUID == request.MenuGUID && a.RoleGUID == userRoleInfo.RoleGuid && a.ApiGUID == null &&
                        a.Menu.MenuApis.Any(b => b.ApiGUID == ApiGUID)
                    );
                }

                if (PRoleAccess == null)
                {
                    throw new ServiceExeption("خطای امنیتی رخ داده است");
                }
            }


            bool Exist = RoleAccessService.Table.Any(a =>
                a.RoleGUID == request.RoleGUID && a.MenuGUID == request.MenuGUID && a.ApiGUID == ApiGUID);
            if (!Exist)
            {
                {
                    RoleAccessService.Table.Add(new RoleAccess()
                    {
                        RoleGUID = request.RoleGUID,
                        ApiGUID = ApiGUID,
                        MenuGUID = request.MenuGUID,
                        PRoleAccessGUID = PRoleAccess?.GUID ?? null,
                    });
                    await RoleAccessService.SaveChangesAsync(cancellationToken);
                }
            }
        }


        await ApiService.UpateApiListInBackground();

        return Guid.Empty;
    }
}