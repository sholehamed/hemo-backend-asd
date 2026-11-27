
using Application.Common.Models;

namespace Application.Core.ShoppingCartStatus_core.Queries;

public record ShoppingCartStatusWithPaginationQuery : PageFilter<ShoppingCartStatus, ShoppingCartStatusWithPaginationDTO>
{

}

public record ShoppingCartStatusWithPaginationDTO : IMapFrom<ShoppingCartStatus>
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("کد")]
    public int Gkey { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<ShoppingCartStatus, ShoppingCartStatusWithPaginationDTO>();
    }
}

public class
    ShoppingCartStatusWithPaginationQueryHandler : IRequestHandler<ShoppingCartStatusWithPaginationQuery,
    PaginatedList<ShoppingCartStatusWithPaginationDTO>>
{
    private ShoppingCartStatusService ShoppingCartStatusService;
    public ShoppingCartStatusWithPaginationQueryHandler(ShoppingCartStatusService ShoppingCartStatusService)
    {
        this.ShoppingCartStatusService = ShoppingCartStatusService;
    }

    public async Task<PaginatedList<ShoppingCartStatusWithPaginationDTO>> Handle(ShoppingCartStatusWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<ShoppingCartStatus> queryable = ShoppingCartStatusService.Table;
        return await request.ToPaging( queryable,ShoppingCartStatusService.mapper);
    }
}

