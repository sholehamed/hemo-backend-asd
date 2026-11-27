

namespace Application.Core.MenuApi_core.Queries;

public record MenuApiFindAllQuery : IRequest<List<MenuApiFindAllDTO>>
{

}

public record MenuApiFindAllDTO : IMapFrom<MenuApi>
{

    [PropConfig("منو")]
    public Guid MenuGUID { get; init; }

    [PropConfig("سرویس")]
    public Guid ApiUID { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<MenuApi, MenuApiFindAllDTO>();
    }
}

public class
    MenuApiFindAllQueryHandler : IRequestHandler<MenuApiFindAllQuery,
    List<MenuApiFindAllDTO>>
{
    private MenuApiService MenuApiService;
    public MenuApiFindAllQueryHandler(MenuApiService MenuApiService)
    {
        this.MenuApiService = MenuApiService;
    }

    public async Task<List<MenuApiFindAllDTO>> Handle(MenuApiFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await MenuApiService.Table.AsNoTracking()
            .ProjectTo<MenuApiFindAllDTO>(MenuApiService.mapper.ConfigurationProvider).ToListAsync();
    }
}



