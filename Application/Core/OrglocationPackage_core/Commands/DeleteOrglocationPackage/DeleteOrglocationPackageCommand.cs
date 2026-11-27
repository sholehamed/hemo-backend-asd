
namespace Application.Core.OrglocationPackage_core.Commands.DeleteOrglocationPackage;

public record DeleteOrglocationPackageCommand(Guid GUID) : IRequest;

public class DeleteOrglocationPackageCommandHandler : IRequestHandler<DeleteOrglocationPackageCommand>
{
    private OrglocationPackageService OrglocationPackageService;

    public DeleteOrglocationPackageCommandHandler(OrglocationPackageService OrglocationPackageService)
    {
        this.OrglocationPackageService = OrglocationPackageService;
    }

    public async Task Handle(DeleteOrglocationPackageCommand request, CancellationToken cancellationToken)
    {
        var entity = await OrglocationPackageService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        OrglocationPackageService.Table.Remove(entity);

        await OrglocationPackageService.SaveChangesAsync(cancellationToken);
    }

}

