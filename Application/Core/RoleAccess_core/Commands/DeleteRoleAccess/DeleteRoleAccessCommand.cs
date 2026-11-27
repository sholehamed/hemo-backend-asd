
namespace Application.Core.RoleAccess_core.Commands.DeleteRoleAccess;

public record DeleteRoleAccessCommand(Guid GUID) : IRequest;

public class DeleteRoleAccessCommandHandler : IRequestHandler<DeleteRoleAccessCommand>
{
    private RoleAccessService RoleAccessService;
    private RoleService RoleService;
    private ApiService ApiService;

    public DeleteRoleAccessCommandHandler(RoleAccessService roleAccessService, RoleService roleService, ApiService apiService)
    {
        RoleAccessService = roleAccessService;
        RoleService = roleService;
        ApiService = apiService;
    }

    public async Task Handle(DeleteRoleAccessCommand request, CancellationToken cancellationToken)
    {
        var entity = await RoleAccessService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        await RoleService.RoleIsValid(entity.RoleGUID);

        List<RoleAccess> roleAccesses = RoleAccessService.Table.Where(a => a.Path.StartsWith(entity.Path)).ToList();
        
        RoleAccessService.Table.RemoveRange(roleAccesses);

        await RoleAccessService.SaveChangesAsync(cancellationToken);
        
        await ApiService.UpateApiListInBackground();
    }

}

