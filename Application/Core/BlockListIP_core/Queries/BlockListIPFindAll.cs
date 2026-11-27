

namespace Application.Core.BlockListIP_core.Queries;

public record BlockListIPFindAllQuery : IRequest<List<BlockListIPFindAllDTO>>
{

}

public record BlockListIPFindAllDTO : IMapFrom<BlockListIP>
{

    [PropConfig("آدرس ای پی")]
    public string IPAddress { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }

    
    



    public void Mapping(Profile profile)
    {
        profile.CreateMap<BlockListIP, BlockListIPFindAllDTO>();
    }
}

public class
    BlockListIPFindAllQueryHandler : IRequestHandler<BlockListIPFindAllQuery,
    List<BlockListIPFindAllDTO>>
{
    private BlockListIPService BlockListIPService;
    public BlockListIPFindAllQueryHandler(BlockListIPService BlockListIPService)
    {
        this.BlockListIPService = BlockListIPService;
    }

    public async Task<List<BlockListIPFindAllDTO>> Handle(BlockListIPFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await BlockListIPService.Table.AsNoTracking()
            .ProjectTo<BlockListIPFindAllDTO>(BlockListIPService.mapper.ConfigurationProvider).ToListAsync();
    }
}



