using Application.Common.Utility.Security;


namespace Application.Core.RoleAccess_core.Commands.RoleAccessCheckPagePermission;

public record RoleAccessCheckPagePermissionCommand : IRequest<bool>, IMapFrom<RoleAccess>
{
    [PropConfig("منو")] public string Menu { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<RoleAccessCheckPagePermissionCommand, RoleAccess>();
    }
}

public class RoleAccessCheckPagePermissionCommandHandler : IRequestHandler<RoleAccessCheckPagePermissionCommand, bool>
{
    private RoleAccessService RoleAccessService;
    private AuthService AuthService;

    public RoleAccessCheckPagePermissionCommandHandler(RoleAccessService roleAccessService, AuthService authService)
    {
        RoleAccessService = roleAccessService;
        AuthService = authService;
    }

    public async Task<bool> Handle(RoleAccessCheckPagePermissionCommand request, CancellationToken cancellationToken)
    {
        long UserRoleGkey = AuthService.GetUserRoleGkey();

        Menu? menu = RoleAccessService.db.Menu.FirstOrDefault(menu1 => menu1.routerLink == request.Menu);

        if (menu == null )
        {
            return false;
        }

        if (menu.IsPublic == true)
        {
            return true;
        }

        Role? Role = RoleAccessService.db.UserRole.Where(a => a.Gkey == UserRoleGkey).Select(a => a.Role)
            .FirstOrDefault();

        if (Role == null)
        {
            return false;
        }
        
        if (Role.Gkey == 1)
        {
            return true;
        }

        bool IsValid = RoleAccessService.Table.Any(a =>
            a.RoleGUID == Role.GUID && a.MenuGUID==menu.GUID);

        return IsValid;
    }
}