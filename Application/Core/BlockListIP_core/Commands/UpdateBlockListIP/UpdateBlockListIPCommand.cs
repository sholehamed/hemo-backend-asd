
namespace Application.Core.BlockListIP_core.Commands.UpdateBlockListIP;

public record UpdateBlockListIPCommand : IRequest, IMapFrom<BlockListIP>
{
 
   [PropConfig("آدرس ای پی")]
   public string IPAddress { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }
 
   
   

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateBlockListIPCommand, BlockListIP>();
    }
}

public class UpdateBlockListIPCommandHandler : IRequestHandler<UpdateBlockListIPCommand>
{
    private BlockListIPService BlockListIPService;

    public UpdateBlockListIPCommandHandler(BlockListIPService BlockListIPService)
    {
        this.BlockListIPService = BlockListIPService;
    }

    public async Task Handle(UpdateBlockListIPCommand request, CancellationToken cancellationToken)
    {
        BlockListIP? OldEntity = await BlockListIPService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        BlockListIPService.mapper.Map(request, OldEntity);
        
        await BlockListIPService.SaveChangesAsync(cancellationToken);
    }
}

