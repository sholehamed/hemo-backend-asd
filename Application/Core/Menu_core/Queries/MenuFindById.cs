namespace Application.Core.Menu_core.Queries;

public record MenuFindByIdQuery(Guid GUID) : IRequest<MenuFindByIdDTO>;

public record MenuFindByIdDTO : IMapFrom<Menu>
{

    [PropConfig("عنوان")] public string label { get; init; }

    [PropConfig("آیکون")] public string icon { get; init; }

    [PropConfig("آدرس")] public string? routerLink { get; init; }

    [PropConfig("ترتیب نمایش")] public int Gorder { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<Menu, MenuFindByIdDTO>();
    }
}

public class
    MenuFindByIdQueryHandler : IRequestHandler<MenuFindByIdQuery, MenuFindByIdDTO>
{
    private readonly MenuService MenuService;

    public MenuFindByIdQueryHandler(MenuService MenuService)
    {
        this.MenuService = MenuService;
    }

    public async Task<MenuFindByIdDTO> Handle(MenuFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        var Data = MenuService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<MenuFindByIdDTO>(MenuService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}