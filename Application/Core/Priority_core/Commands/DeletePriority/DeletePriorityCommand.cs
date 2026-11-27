
namespace Application.Core.Priority_core.Commands.DeletePriority;

public record DeletePriorityCommand(Guid GUID) : IRequest;

public class DeletePriorityCommandHandler : IRequestHandler<DeletePriorityCommand>
{
    private PriorityService PriorityService;

    public DeletePriorityCommandHandler(PriorityService PriorityService)
    {
        this.PriorityService = PriorityService;
    }

    public async Task Handle(DeletePriorityCommand request, CancellationToken cancellationToken)
    {
        var entity = await PriorityService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        PriorityService.Table.Remove(entity);

        await PriorityService.SaveChangesAsync(cancellationToken);
    }

}

