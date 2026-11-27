
namespace Application.Core.OrgType_core.Commands.DeleteOrgType;

public record DeleteOrgTypeCommand(Guid GUID) : IRequest;

public class DeleteOrgTypeCommandHandler : IRequestHandler<DeleteOrgTypeCommand>
{
    private OrgTypeService OrgTypeService;

    public DeleteOrgTypeCommandHandler(OrgTypeService OrgTypeService)
    {
        this.OrgTypeService = OrgTypeService;
    }

    public async Task Handle(DeleteOrgTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await OrgTypeService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        OrgTypeService.Table.Remove(entity);

        await OrgTypeService.SaveChangesAsync(cancellationToken);
    }

}

