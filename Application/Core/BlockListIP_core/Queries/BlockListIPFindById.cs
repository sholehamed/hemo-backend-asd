
namespace Application.Core.BlockListIP_core.Queries;

public record BlockListIPFindByIdQuery(Guid GUID) : IRequest<BlockListIPFindByIdDTO>;

public record BlockListIPFindByIdDTO : IMapFrom<BlockListIP>
{
    
    [PropConfig("آدرس ای پی")]
    public string IPAddress { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }
    
    
    



    public void Mapping(Profile profile)
    {
        profile.CreateMap<BlockListIP, BlockListIPFindByIdDTO>();
    }
}

public class
    BlockListIPFindByIdQueryHandler : IRequestHandler<BlockListIPFindByIdQuery, BlockListIPFindByIdDTO>
{
    private BlockListIPService BlockListIPService;

    public BlockListIPFindByIdQueryHandler(BlockListIPService BlockListIPService)
    {
        this.BlockListIPService = BlockListIPService;
    }

    public async Task<BlockListIPFindByIdDTO> Handle(BlockListIPFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        BlockListIPFindByIdDTO? Data = BlockListIPService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<BlockListIPFindByIdDTO>(BlockListIPService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

