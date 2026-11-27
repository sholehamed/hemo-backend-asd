
namespace Application.Core.OrgLocationType_core.Commands.DeleteOrgLocationType;

public record DeleteOrgLocationTypeCommand(Guid GUID) : IRequest;

public class DeleteOrgLocationTypeCommandHandler : IRequestHandler<DeleteOrgLocationTypeCommand>
{
    private OrgLocationTypeService OrgLocationTypeService;

    public DeleteOrgLocationTypeCommandHandler(OrgLocationTypeService OrgLocationTypeService)
    {
        this.OrgLocationTypeService = OrgLocationTypeService;
    }

    public async Task Handle(DeleteOrgLocationTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await OrgLocationTypeService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        OrgLocationTypeService.Table.Remove(entity);

        await OrgLocationTypeService.SaveChangesAsync(cancellationToken);
    }

}

