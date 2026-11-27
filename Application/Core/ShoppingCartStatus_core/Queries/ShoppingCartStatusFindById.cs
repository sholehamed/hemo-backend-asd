
namespace Application.Core.ShoppingCartStatus_core.Queries;

public record ShoppingCartStatusFindByIdQuery(Guid GUID) : IRequest<ShoppingCartStatusFindByIdDTO>;

public record ShoppingCartStatusFindByIdDTO : IMapFrom<ShoppingCartStatus>
{
    
    [PropConfig("عنوان")]
    public string Name { get; init; }
    
    [PropConfig("کد")]
    public int Gkey { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<ShoppingCartStatus, ShoppingCartStatusFindByIdDTO>();
    }
}

public class
    ShoppingCartStatusFindByIdQueryHandler : IRequestHandler<ShoppingCartStatusFindByIdQuery, ShoppingCartStatusFindByIdDTO>
{
    private ShoppingCartStatusService ShoppingCartStatusService;

    public ShoppingCartStatusFindByIdQueryHandler(ShoppingCartStatusService ShoppingCartStatusService)
    {
        this.ShoppingCartStatusService = ShoppingCartStatusService;
    }

    public async Task<ShoppingCartStatusFindByIdDTO> Handle(ShoppingCartStatusFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        ShoppingCartStatusFindByIdDTO? Data = ShoppingCartStatusService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<ShoppingCartStatusFindByIdDTO>(ShoppingCartStatusService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

