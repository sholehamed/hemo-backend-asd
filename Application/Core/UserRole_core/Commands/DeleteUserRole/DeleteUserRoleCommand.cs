
using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Core.UserRole_core.Commands.DeleteUserRole;

public record DeleteUserRoleCommand(Guid GUID) : IRequest;

public class DeleteUserRoleCommandHandler : IRequestHandler<DeleteUserRoleCommand>
{
    private UserRoleService UserRoleService;
    private OrgLocationService OrgLocationService;
    private RoleService RoleService;

    public DeleteUserRoleCommandHandler(UserRoleService userRoleService, OrgLocationService orgLocationService, RoleService roleService)
    {
        UserRoleService = userRoleService;
        OrgLocationService = orgLocationService;
        RoleService = roleService;
    }

    public async Task Handle(DeleteUserRoleCommand request, CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        var entity = await UserRoleService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        if (entity.OrgLocationGUID != userRoleInfo.OrgLocationGUID&& !userRoleInfo.IsSuperAdmin)
        {
            throw new ServiceExeption("خطای امنیتی رخ داده است");
        }
        Guard.Against.NotFound(request.GUID, entity);
        await RoleService.RoleIsValid(entity.RoleGUID);

        bool CheckHasAccessToChild = await OrgLocationService.HasAccessToChild(userRoleInfo.OrgLocationGUID, entity.OrgLocationGUID);
        if (!CheckHasAccessToChild)
        {
            throw new ServiceExeption("دسترسی غیرمجاز اطلاعات");
        }

        UserRoleService.Table.Remove(entity);

        await UserRoleService.SaveChangesAsync(cancellationToken);
    }

}

