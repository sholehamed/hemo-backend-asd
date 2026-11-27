
namespace Application.Core.BlockListIP_core.Commands.DeleteBlockListIP;

public record DeleteBlockListIPCommand(Guid GUID) : IRequest;

public class DeleteBlockListIPCommandHandler : IRequestHandler<DeleteBlockListIPCommand>
{
    private BlockListIPService BlockListIPService;

    public DeleteBlockListIPCommandHandler(BlockListIPService BlockListIPService)
    {
        this.BlockListIPService = BlockListIPService;
    }

    public async Task Handle(DeleteBlockListIPCommand request, CancellationToken cancellationToken)
    {
        var entity = await BlockListIPService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        BlockListIPService.Table.Remove(entity);

        await BlockListIPService.SaveChangesAsync(cancellationToken);
    }

}

