
using Application.Common.Models;

namespace Application.Core.ShoppingCartProduct_core.Queries;

public record ShoppingCartProductWithPaginationQuery : PageFilter<ShoppingCartProduct, ShoppingCartProductWithPaginationDTO>
{

}

public record ShoppingCartProductWithPaginationDTO : IMapFrom<ShoppingCartProduct>
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
        profile.CreateMap<ShoppingCartProduct, ShoppingCartProductWithPaginationDTO>();
    }
}

public class
    ShoppingCartProductWithPaginationQueryHandler : IRequestHandler<ShoppingCartProductWithPaginationQuery,
    PaginatedList<ShoppingCartProductWithPaginationDTO>>
{
    private ShoppingCartProductService ShoppingCartProductService;
    public ShoppingCartProductWithPaginationQueryHandler(ShoppingCartProductService ShoppingCartProductService)
    {
        this.ShoppingCartProductService = ShoppingCartProductService;
    }

    public async Task<PaginatedList<ShoppingCartProductWithPaginationDTO>> Handle(ShoppingCartProductWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<ShoppingCartProduct> queryable = ShoppingCartProductService.Table;
        return await request.ToPaging( queryable,ShoppingCartProductService.mapper);
    }
}

