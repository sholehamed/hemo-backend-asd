
using Application.Common.Models;

namespace Application.Core.UserInvite_core.Commands.DeleteUserInvite;

public record DeleteUserInviteCommand(Guid GUID) : IRequest;

public class DeleteUserInviteCommandHandler : IRequestHandler<DeleteUserInviteCommand>
{
    private UserInviteService UserInviteService;
    private UserRoleService UserRoleService;

    public DeleteUserInviteCommandHandler(UserInviteService userInviteService, UserRoleService userRoleService)
    {
        UserInviteService = userInviteService;
        UserRoleService = userRoleService;
    }

    public async Task Handle(DeleteUserInviteCommand request, CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        
        var entity = await UserInviteService.Table.FirstOrDefaultAsync(a=>a.GUID==request.GUID&&a.OrgLocation.Path.StartsWith(userRoleInfo.OrgLocationPath),cancellationToken);
        
        Guard.Against.NotFound(request.GUID, entity);

        UserInviteService.Table.Remove(entity);

        await UserInviteService.SaveChangesAsync(cancellationToken);
    }

}

