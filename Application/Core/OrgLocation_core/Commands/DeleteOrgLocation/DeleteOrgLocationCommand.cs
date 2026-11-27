
namespace Application.Core.OrgLocation_core.Commands.DeleteOrgLocation;

public record DeleteOrgLocationCommand(Guid GUID) : IRequest;

public class DeleteOrgLocationCommandHandler : IRequestHandler<DeleteOrgLocationCommand>
{
    private OrgLocationService OrgLocationService;

    public DeleteOrgLocationCommandHandler(OrgLocationService OrgLocationService)
    {
        this.OrgLocationService = OrgLocationService;
    }

    public async Task Handle(DeleteOrgLocationCommand request, CancellationToken cancellationToken)
    {
        await OrgLocationService.OrgLocationIsValid(request.GUID);
        
        var entity = await OrgLocationService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        List<OrgLocation> orgLocations = OrgLocationService.Table.Where(a => a.Path.StartsWith(entity.Path)).ToList();

        OrgLocationService.Table.RemoveRange(orgLocations);

        await OrgLocationService.SaveChangesAsync(cancellationToken);
    }

}

