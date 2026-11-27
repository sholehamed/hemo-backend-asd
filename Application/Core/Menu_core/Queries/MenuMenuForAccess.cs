using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Core.Menu_core.Queries;

public record MenuMenuForAccessQuery(Guid OrglocationPackageGUID, Guid RoleGUID) : IRequest<List<MenuMenuForAccessDTO>>
{
}

public record MenuMenuForAccessDTO : TreeNode<MenuMenuForAccessData, MenuMenuForAccessDTO>, IMapFrom<Menu>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Menu, MenuMenuForAccessDTO>()
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.label))
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.GUID))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(a => new MenuMenuForAccessData
            {
            }));
    }
}

public record MenuMenuForAccessData
{
}

public class
    MenuMenuForAccessQueryHandler : IRequestHandler<MenuMenuForAccessQuery,
    List<MenuMenuForAccessDTO>>
{
    private MenuService MenuService;
    private RoleService RoleService;
    private UserRoleService UserRoleService;
    private OrglocationPackageService OrglocationPackageService;

    public MenuMenuForAccessQueryHandler(MenuService menuService, RoleService roleService,
        UserRoleService userRoleService, OrglocationPackageService orglocationPackageService)
    {
        MenuService = menuService;
        RoleService = roleService;
        UserRoleService = userRoleService;
        OrglocationPackageService = orglocationPackageService;
    }

    public async Task<List<MenuMenuForAccessDTO>> Handle(MenuMenuForAccessQuery request,
        CancellationToken cancellationToken)
    {
        await RoleService.RoleIsValid(request.RoleGUID);

        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();

        OrglocationPackage orglocationPackage =
            await OrglocationPackageService.GetValidOrglocationPackage(request.OrglocationPackageGUID,
                userRoleInfo.OrgLocationGUID);

        IQueryable<Menu> queryable = MenuService.Table
            .Where(a =>
                MenuService.db.Menu.Any(b =>
                    b.Path.StartsWith(a.Path) &&
                    b.RoleAccess.Any(c => c.ApiGUID == null && c.RoleGUID == orglocationPackage.Package.RoleGUID))
            ).AsNoTracking();

        List<MenuMenuForAccessDTO> Data = await queryable.Where(a => a.PMenuGUID == null)
            .ProjectTo<MenuMenuForAccessDTO>(MenuService.mapper.ConfigurationProvider).ToListAsync();

        foreach (var Item in Data)
        {
            await SetChild(queryable, Item, request.RoleGUID);
        }

        return Data;
    }

    public async Task SetChild(IQueryable<Menu> queryable, MenuMenuForAccessDTO Item, Guid TargetRole)
    {
        Item.Children = await queryable.Where(a =>
                a.PMenuGUID == Item.Key
            ).OrderBy(a => a.Created).ProjectTo<MenuMenuForAccessDTO>(MenuService.mapper.ConfigurationProvider)
            .ToListAsync();

        Item.Expanded = true;
        Item.Checked =
            MenuService.db.RoleAccess.Any(a => a.RoleGUID == TargetRole && a.MenuGUID == Item.Key && a.ApiGUID == null);
        Item.Leaf = !Item.Children.Any();
        foreach (var child in Item.Children)
        {
            await SetChild(queryable, child, TargetRole);
        }
    }
}