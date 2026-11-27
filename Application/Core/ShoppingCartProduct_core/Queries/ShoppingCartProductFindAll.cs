

namespace Application.Core.ShoppingCartProduct_core.Queries;

public record ShoppingCartProductFindAllQuery : IRequest<List<ShoppingCartProductFindAllDTO>>
{

}

public record ShoppingCartProductFindAllDTO : IMapFrom<ShoppingCartProduct>
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
        profile.CreateMap<ShoppingCartProduct, ShoppingCartProductFindAllDTO>();
    }
}

public class
    ShoppingCartProductFindAllQueryHandler : IRequestHandler<ShoppingCartProductFindAllQuery,
    List<ShoppingCartProductFindAllDTO>>
{
    private ShoppingCartProductService ShoppingCartProductService;
    public ShoppingCartProductFindAllQueryHandler(ShoppingCartProductService ShoppingCartProductService)
    {
        this.ShoppingCartProductService = ShoppingCartProductService;
    }

    public async Task<List<ShoppingCartProductFindAllDTO>> Handle(ShoppingCartProductFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await ShoppingCartProductService.Table.AsNoTracking()
            .ProjectTo<ShoppingCartProductFindAllDTO>(ShoppingCartProductService.mapper.ConfigurationProvider).ToListAsync();
    }
}



