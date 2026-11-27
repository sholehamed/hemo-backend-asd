
using Application.Common.Models;

namespace Application.Core.BlockListIP_core.Queries;

public record BlockListIPWithPaginationQuery : PageFilter<BlockListIP, BlockListIPWithPaginationDTO>
{

}

public record BlockListIPWithPaginationDTO : IMapFrom<BlockListIP>
{

    [PropConfig("آدرس ای پی")]
    public string IPAddress { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }

    
    


    public void Mapping(Profile profile)
    {
        profile.CreateMap<BlockListIP, BlockListIPWithPaginationDTO>();
    }
}

public class
    BlockListIPWithPaginationQueryHandler : IRequestHandler<BlockListIPWithPaginationQuery,
    PaginatedList<BlockListIPWithPaginationDTO>>
{
    private BlockListIPService BlockListIPService;
    public BlockListIPWithPaginationQueryHandler(BlockListIPService BlockListIPService)
    {
        this.BlockListIPService = BlockListIPService;
    }

    public async Task<PaginatedList<BlockListIPWithPaginationDTO>> Handle(BlockListIPWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<BlockListIP> queryable = BlockListIPService.Table;
        return await request.ToPaging( queryable,BlockListIPService.mapper);
    }
}

