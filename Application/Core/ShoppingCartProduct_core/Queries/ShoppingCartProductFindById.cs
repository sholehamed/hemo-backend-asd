
namespace Application.Core.ShoppingCartProduct_core.Queries;

public record ShoppingCartProductFindByIdQuery(Guid GUID) : IRequest<ShoppingCartProductFindByIdDTO>;

public record ShoppingCartProductFindByIdDTO : IMapFrom<ShoppingCartProduct>
{
    
    [PropConfig("پکیج")]
    public Guid PackageGUID { get; init; }
    
    [PropConfig("سبد خرید")]
    public Guid ShoppingCartGUID { get; init; }
    
    [PropConfig("شرکت")]
    public Guid OrgLocationGUID { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<ShoppingCartProduct, ShoppingCartProductFindByIdDTO>();
    }
}

public class
    ShoppingCartProductFindByIdQueryHandler : IRequestHandler<ShoppingCartProductFindByIdQuery, ShoppingCartProductFindByIdDTO>
{
    private ShoppingCartProductService ShoppingCartProductService;

    public ShoppingCartProductFindByIdQueryHandler(ShoppingCartProductService ShoppingCartProductService)
    {
        this.ShoppingCartProductService = ShoppingCartProductService;
    }

    public async Task<ShoppingCartProductFindByIdDTO> Handle(ShoppingCartProductFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        ShoppingCartProductFindByIdDTO? Data = ShoppingCartProductService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<ShoppingCartProductFindByIdDTO>(ShoppingCartProductService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

