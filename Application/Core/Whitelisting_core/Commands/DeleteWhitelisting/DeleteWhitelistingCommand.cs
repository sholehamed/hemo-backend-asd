
namespace Application.Core.Whitelisting_core.Commands.DeleteWhitelisting;

public record DeleteWhitelistingCommand(Guid GUID) : IRequest;

public class DeleteWhitelistingCommandHandler : IRequestHandler<DeleteWhitelistingCommand>
{
    private WhitelistingService WhitelistingService;

    public DeleteWhitelistingCommandHandler(WhitelistingService WhitelistingService)
    {
        this.WhitelistingService = WhitelistingService;
    }

    public async Task Handle(DeleteWhitelistingCommand request, CancellationToken cancellationToken)
    {
        var entity = await WhitelistingService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        WhitelistingService.Table.Remove(entity);

        await WhitelistingService.SaveChangesAsync(cancellationToken);
    }

}

