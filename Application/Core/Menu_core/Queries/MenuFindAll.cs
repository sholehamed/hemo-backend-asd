namespace Application.Core.Menu_core.Queries;

public record MenuFindAllQuery : IRequest<List<MenuFindAllDTO>>
{
}

public record MenuFindAllDTO : IMapFrom<Menu>
{
    [PropConfig("منو پدر")] public Guid? PMenuGUID { get; init; }
    
    [PropConfig("عنوان")] public string label { get; init; }

    [PropConfig("آیکون")] public string icon { get; init; }

    [PropConfig("آدرس")] public string? routerLink { get; init; }

    [PropConfig("ترتیب نمایش")] public int Gorder { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<Menu, MenuFindAllDTO>();
    }
}

public class
    MenuFindAllQueryHandler : IRequestHandler<MenuFindAllQuery,
    List<MenuFindAllDTO>>
{
    private readonly MenuService MenuService;

    public MenuFindAllQueryHandler(MenuService MenuService)
    {
        this.MenuService = MenuService;
    }

    public async Task<List<MenuFindAllDTO>> Handle(MenuFindAllQuery request,
        CancellationToken cancellationToken)
    {
        return await MenuService.Table.AsNoTracking()
            .ProjectTo<MenuFindAllDTO>(MenuService.mapper.ConfigurationProvider).ToListAsync();
    }
}