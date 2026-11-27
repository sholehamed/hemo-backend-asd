using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Core.Role_core.Commands.DeleteRole;

public record DeleteRoleCommand(Guid GUID) : IRequest;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly RoleService RoleService;
    private UserRoleService UserRoleService;

    public DeleteRoleCommandHandler(RoleService roleService, UserRoleService userRoleService)
    {
        RoleService = roleService;
        UserRoleService = userRoleService;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        await RoleService.RoleIsValid(request.GUID);

        var entity = await RoleService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();

        if (entity.OrgLocationGUID != userRoleInfo.OrgLocationGUID)
        {
            throw new ServiceExeption("خطای امنیتی رخ داده است");
        }

        List<Role> childs = RoleService.Table
            .Where(a => a.OrgLocationGUID == userRoleInfo.OrgLocationGUID && a.Path.StartsWith(entity.Path)).ToList();
       
        RoleService.Table.RemoveRange(childs);

        await RoleService.SaveChangesAsync(cancellationToken);
    }
}