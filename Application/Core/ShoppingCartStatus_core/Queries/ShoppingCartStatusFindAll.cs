

namespace Application.Core.ShoppingCartStatus_core.Queries;

public record ShoppingCartStatusFindAllQuery : IRequest<List<ShoppingCartStatusFindAllDTO>>
{

}

public record ShoppingCartStatusFindAllDTO : IMapFrom<ShoppingCartStatus>
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("کد")]
    public int Gkey { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<ShoppingCartStatus, ShoppingCartStatusFindAllDTO>();
    }
}

public class
    ShoppingCartStatusFindAllQueryHandler : IRequestHandler<ShoppingCartStatusFindAllQuery,
    List<ShoppingCartStatusFindAllDTO>>
{
    private ShoppingCartStatusService ShoppingCartStatusService;
    public ShoppingCartStatusFindAllQueryHandler(ShoppingCartStatusService ShoppingCartStatusService)
    {
        this.ShoppingCartStatusService = ShoppingCartStatusService;
    }

    public async Task<List<ShoppingCartStatusFindAllDTO>> Handle(ShoppingCartStatusFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await ShoppingCartStatusService.Table.AsNoTracking()
            .ProjectTo<ShoppingCartStatusFindAllDTO>(ShoppingCartStatusService.mapper.ConfigurationProvider).ToListAsync();
    }
}



