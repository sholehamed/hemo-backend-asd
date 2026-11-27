namespace Application.Core.Menu_core.Queries;

public record MenuGetAllRouteQuery : IRequest<List<MenuGetAllRouteDTO>>
{

}

public record MenuGetAllRouteDTO : IMapFrom<Menu>
{

    [PropConfig("آدرس")]
    public string? routerLink { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Menu, MenuGetAllRouteDTO>();
    }
}

public class
    MenuGetAllRouteQueryHandler : IRequestHandler<MenuGetAllRouteQuery,
    List<MenuGetAllRouteDTO>>
{
    private MenuService MenuService;
    public MenuGetAllRouteQueryHandler(MenuService MenuService)
    {
        this.MenuService = MenuService;
    }

    public async Task<List<MenuGetAllRouteDTO>> Handle(MenuGetAllRouteQuery request,
        CancellationToken cancellationToken)
    {
        
        return await MenuService.Table.AsNoTracking()
            .ProjectTo<MenuGetAllRouteDTO>(MenuService.mapper.ConfigurationProvider).ToListAsync();
    }
}