
namespace Application.Core.MenuApi_core.Queries;

public record MenuApiFindByIdQuery(Guid GUID) : IRequest<MenuApiFindByIdDTO>;

public record MenuApiFindByIdDTO : IMapFrom<MenuApi>
{
    
    [PropConfig("منو")]
    public Guid MenuGUID { get; init; }
    
    [PropConfig("سرویس")]
    public Guid ApiUID { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<MenuApi, MenuApiFindByIdDTO>();
    }
}

public class
    MenuApiFindByIdQueryHandler : IRequestHandler<MenuApiFindByIdQuery, MenuApiFindByIdDTO>
{
    private MenuApiService MenuApiService;

    public MenuApiFindByIdQueryHandler(MenuApiService MenuApiService)
    {
        this.MenuApiService = MenuApiService;
    }

    public async Task<MenuApiFindByIdDTO> Handle(MenuApiFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        MenuApiFindByIdDTO? Data = MenuApiService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<MenuApiFindByIdDTO>(MenuApiService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

