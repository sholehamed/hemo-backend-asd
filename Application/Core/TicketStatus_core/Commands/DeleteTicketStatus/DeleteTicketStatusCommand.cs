
namespace Application.Core.TicketStatus_core.Commands.DeleteTicketStatus;

public record DeleteTicketStatusCommand(Guid GUID) : IRequest;

public class DeleteTicketStatusCommandHandler : IRequestHandler<DeleteTicketStatusCommand>
{
    private TicketStatusService TicketStatusService;

    public DeleteTicketStatusCommandHandler(TicketStatusService TicketStatusService)
    {
        this.TicketStatusService = TicketStatusService;
    }

    public async Task Handle(DeleteTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await TicketStatusService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        TicketStatusService.Table.Remove(entity);

        await TicketStatusService.SaveChangesAsync(cancellationToken);
    }

}

