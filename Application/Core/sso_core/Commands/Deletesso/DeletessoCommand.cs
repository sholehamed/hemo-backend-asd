
namespace Application.Core.sso_core.Commands.Deletesso;

public record DeletessoCommand(Guid GUID) : IRequest;

public class DeletessoCommandHandler : IRequestHandler<DeletessoCommand>
{
    private ssoService ssoService;

    public DeletessoCommandHandler(ssoService ssoService)
    {
        this.ssoService = ssoService;
    }

    public async Task Handle(DeletessoCommand request, CancellationToken cancellationToken)
    {
        var entity = await ssoService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        ssoService.Table.Remove(entity);

        await ssoService.SaveChangesAsync(cancellationToken);
    }

}

