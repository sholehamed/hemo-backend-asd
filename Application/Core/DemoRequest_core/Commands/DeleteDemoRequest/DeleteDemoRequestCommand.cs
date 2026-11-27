
namespace Application.Core.DemoRequest_core.Commands.DeleteDemoRequest;

public record DeleteDemoRequestCommand(Guid GUID) : IRequest;

public class DeleteDemoRequestCommandHandler : IRequestHandler<DeleteDemoRequestCommand>
{
    private DemoRequestService DemoRequestService;

    public DeleteDemoRequestCommandHandler(DemoRequestService DemoRequestService)
    {
        this.DemoRequestService = DemoRequestService;
    }

    public async Task Handle(DeleteDemoRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await DemoRequestService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        DemoRequestService.Table.Remove(entity);

        await DemoRequestService.SaveChangesAsync(cancellationToken);
    }

}

