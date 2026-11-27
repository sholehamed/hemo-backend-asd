using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Repository.Services;

public class UserRoleService : BaseRepository<UserRole>
{
    private AuthService AuthService;

    public UserRoleService(IApplicationDbContext context, IMapper mapper, AuthService authService) : base(context,
        mapper)
    {
        AuthService = authService;
    }

    public async Task<UserRoleInfo> GetUserRoleInfo()
    {
        long UserRoleGkey = AuthService.GetUserRoleGkey();
        return await GetUserRoleInfoByGkey(UserRoleGkey);
    }

    public async Task<UserRoleInfo> GetUserRoleInfoByGkey(long UserRoleGkey)
    {
        if (UserRoleGkey == 0)
        {
            throw new ServiceExeption("نقش کاربر یافت نشد");
        }

        UserRole userRole = Table.Include(a => a.User).Include(a => a.Role).Include(a => a.OrgLocation)
            .First(a => a.Gkey == UserRoleGkey);
        UserRoleInfo UserRoleInfo = new UserRoleInfo()
        {
            OrgLocationGUID = userRole.OrgLocationGUID,
            RoleGuid = userRole.RoleGUID,
            UserGuid = userRole.UserGUID,
            OrgLocationPath = userRole.OrgLocation?.Path ?? "/",
            RolePath = userRole.Role?.Path ?? "/",
            IsSuperAdmin = userRole.Role.Gkey == 1,
            UserLastName = userRole.User.LastName,
            UserFirstName = userRole.User.FirstName,
            RoleName = userRole.Role.Name,
            ExpireDate = userRole.ExpireDate,
            RoleGkey = userRole.Role.Gkey,
            OrgLocationGkey = userRole.OrgLocation.Gkey,
        };
        return UserRoleInfo;
    }
}