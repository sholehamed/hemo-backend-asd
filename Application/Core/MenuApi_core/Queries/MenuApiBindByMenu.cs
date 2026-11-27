using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Core.MenuApi_core.Queries;

public record MenuApiBindByMenuQuery(Guid MenuGUID) : IRequest<List<MenuApiBindByMenuDTO>>;

public record MenuApiBindByMenuDTO : IMapFrom<MenuApi>
{
    [PropConfig("کد سرویس")] public Guid? ApiGUID { get; init; }
    [PropConfig("عنوان سرویس")] public string ApiName { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<MenuApi, MenuApiBindByMenuDTO>()
            .ForMember(dest => dest.ApiGUID, opt => opt.MapFrom(a => a.ApiGUID))
            .ForMember(dest => dest.ApiName, opt => opt.MapFrom(a => a.Api.Title));
    }
}

public class
    MenuApiBindByMenuQueryHandler : IRequestHandler<MenuApiBindByMenuQuery,
    List<MenuApiBindByMenuDTO>>
{
    private MenuApiService MenuApiService;
    private UserRoleService UserRoleService;

    public MenuApiBindByMenuQueryHandler(MenuApiService menuApiService, UserRoleService userRoleService)
    {
        MenuApiService = menuApiService;
        UserRoleService = userRoleService;
    }

    public async Task<List<MenuApiBindByMenuDTO>> Handle(MenuApiBindByMenuQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        List<MenuApiBindByMenuDTO> Data = await MenuApiService.Table.Where(a =>
                a.Api.IsPublic == false && a.MenuGUID == request.MenuGUID &&
                (a.Api.RoleAccess.Any(b => b.MenuGUID == request.MenuGUID && b.RoleGUID == userRoleInfo.RoleGuid) ||
                 (a.Menu.RoleAccess.Any(b=>b.RoleGUID==userRoleInfo.RoleGuid&&b.ApiGUID==null))||
                 userRoleInfo.IsSuperAdmin)).AsNoTracking()
            .ProjectTo<MenuApiBindByMenuDTO>(MenuApiService.mapper.ConfigurationProvider).ToListAsync();

        if (userRoleInfo.IsSuperAdmin || MenuApiService.db.RoleAccess.Any(a =>
                a.RoleGUID == userRoleInfo.RoleGuid && a.MenuGUID == request.MenuGUID && a.ApiGUID == null))
        {
            Data.Insert(0, new MenuApiBindByMenuDTO() { ApiName = "دسترسی کامل به صفحه", ApiGUID = null });
        }

        return Data;
    }
}