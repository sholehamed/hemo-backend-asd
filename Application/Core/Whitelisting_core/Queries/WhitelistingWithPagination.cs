
using Application.Common.Models;

namespace Application.Core.Whitelisting_core.Queries;

public record WhitelistingWithPaginationQuery : PageFilter<Whitelisting, WhitelistingWithPaginationDTO>
{

}

public record WhitelistingWithPaginationDTO : IMapFrom<Whitelisting>
{

    [PropConfig("آدرس ای پی")]
    public string IPAddress { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }

    
    


    public void Mapping(Profile profile)
    {
        profile.CreateMap<Whitelisting, WhitelistingWithPaginationDTO>();
    }
}

public class
    WhitelistingWithPaginationQueryHandler : IRequestHandler<WhitelistingWithPaginationQuery,
    PaginatedList<WhitelistingWithPaginationDTO>>
{
    private WhitelistingService WhitelistingService;
    public WhitelistingWithPaginationQueryHandler(WhitelistingService WhitelistingService)
    {
        this.WhitelistingService = WhitelistingService;
    }

    public async Task<PaginatedList<WhitelistingWithPaginationDTO>> Handle(WhitelistingWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Whitelisting> queryable = WhitelistingService.Table;
        return await request.ToPaging( queryable,WhitelistingService.mapper);
    }
}

