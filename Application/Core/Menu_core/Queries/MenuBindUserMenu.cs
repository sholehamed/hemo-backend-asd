using Application.Common.Models;
using Application.Common.Utility.Security;
using Guid = System.Guid;

namespace Application.Core.Menu_core.Queries;

public record MenuBindUserMenuQuery(string SubPath) : IRequest<List<MenuBindUserMenuDTO>>
{
}

public record MenuBindUserMenuDTO : IMapFrom<Menu>
{
    [PropConfig("کلید")] public Guid GUID { get; init; }
    [PropConfig("عنوان")] public string label { get; init; }

    [PropConfig("آیکون")] public string icon { get; init; }

    [PropConfig("آدرس")] public string? routerLink { get; init; }

    public List<MenuBindUserMenuDTO>? items { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Menu, MenuBindUserMenuDTO>()
            .ForMember(dest => dest.routerLink, opt =>
                opt.MapFrom(src => src.Menus.Any(a=>a.ShowInMenu) ? "" : src.routerLink))
            ;
    }
}

public class
    MenuBindUserMenuQueryHandler : IRequestHandler<MenuBindUserMenuQuery,
    List<MenuBindUserMenuDTO>>
{
    private readonly MenuService MenuService;
    private UserRoleService UserRoleService;

    public MenuBindUserMenuQueryHandler(MenuService menuService, UserRoleService userRoleService)
    {
        MenuService = menuService;
        UserRoleService = userRoleService;
    }

    public async Task<List<MenuBindUserMenuDTO>> Handle(MenuBindUserMenuQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        IQueryable<Menu> queryable = MenuService.db.Menu.AsNoTracking().Where(a => a.ShowInMenu
            && (
                userRoleInfo.IsSuperAdmin ||
                a.IsPublic ||
                a.RoleAccess.Any(b => b.RoleGUID == userRoleInfo.RoleGuid) ||
                MenuService.db.Menu.Any(b =>
                    b.Path.StartsWith(a.Path) && b.ShowInMenu == true &&
                    (b.RoleAccess.Any(c => c.RoleGUID == userRoleInfo.RoleGuid) || b.IsPublic))
            )
        );

        List<MenuBindUserMenuDTO> Data = await queryable
            .Where(a => a.PMenuGUID == null &&
                        (a.routerLink == request.SubPath || string.IsNullOrWhiteSpace(request.SubPath)))
            .OrderBy(a => a.Gorder)
            .ProjectTo<MenuBindUserMenuDTO>(MenuService.mapper.ConfigurationProvider).ToListAsync();
        foreach (var item in Data) await SetChild(queryable, item);

        return Data;
    }

    public async Task SetChild(IQueryable<Menu> queryable, MenuBindUserMenuDTO Menu)
    {
        Menu.items = await queryable.Where(a => a.ShowInMenu && a.PMenuGUID == Menu.GUID).OrderBy(a => a.Gorder)
            .ProjectTo<MenuBindUserMenuDTO>(MenuService.mapper.ConfigurationProvider).ToListAsync();
        foreach (var item in Menu.items) await SetChild(queryable, item);

        if (!Menu.items.Any()) Menu.items = null;
    }
}