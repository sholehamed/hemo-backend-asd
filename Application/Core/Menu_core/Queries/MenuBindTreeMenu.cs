using Application.Common.Models;

namespace Application.Core.Menu_core.Queries;

public record MenuBindTreeMenuQuery : IRequest<List<MenuBindTreeMenuDTO>>
{
}

public record MenuBindTreeMenuDTO : TreeNode<MenuBindTreeMenuData, MenuBindTreeMenuDTO>, IMapFrom<Menu>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Menu, MenuBindTreeMenuDTO>()
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.label))
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.GUID));
    }
}

public record MenuBindTreeMenuData
{
}

public class
    MenuBindTreeMenuQueryHandler : IRequestHandler<MenuBindTreeMenuQuery,
    List<MenuBindTreeMenuDTO>>
{
    private MenuService MenuService;
    private UserRoleService UserRoleService;

    public MenuBindTreeMenuQueryHandler(MenuService menuService, UserRoleService userRoleService)
    {
        MenuService = menuService;
        UserRoleService = userRoleService;
    }

    public async Task<List<MenuBindTreeMenuDTO>> Handle(MenuBindTreeMenuQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        IQueryable<Menu> queryable = MenuService.Table.AsNoTracking().Where(a =>
            
            userRoleInfo.IsSuperAdmin ||
            a.RoleAccess.Any(b => b.RoleGUID == userRoleInfo.RoleGuid) ||
            MenuService.db.Menu.Any(b =>
                b.Path.StartsWith(a.Path) && 
                b.RoleAccess.Any(c => c.RoleGUID == userRoleInfo.RoleGuid))
        );
        List<MenuBindTreeMenuDTO> Data = await queryable.Where(a => a.PMenuGUID == null)
            .ProjectTo<MenuBindTreeMenuDTO>(MenuService.mapper.ConfigurationProvider).OrderBy(a=>a.Label).ToListAsync();
        foreach (var Item in Data)
        {
            await SetChild(queryable, Item);
        }

        return Data;
    }

    public async Task SetChild(IQueryable<Menu> queryable, MenuBindTreeMenuDTO Item)
    {
        Item.Children = await queryable.Where(a =>
                a.PMenuGUID == Item.Key
            ).OrderBy(a => a.Created).ProjectTo<MenuBindTreeMenuDTO>(MenuService.mapper.ConfigurationProvider).OrderBy(a=>a.Label)
            .ToListAsync();

        Item.Expanded = false;
        Item.Leaf = !Item.Children.Any();
        foreach (var child in Item.Children)
        {
            await SetChild(queryable, child);
        }
    }
}