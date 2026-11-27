using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Core.User_core.Queries;

public record UserRefreshTokenQuery() : IRequest<Token>;

public class
    UserRefreshTokenQueryHandler : IRequestHandler<UserRefreshTokenQuery, Token>
{
    private UserService UserService;
    private AuthService AuthService;
    private SystemSettingService SystemSettingService;

    public UserRefreshTokenQueryHandler(UserService userService, AuthService authService, SystemSettingService systemSettingService)
    {
        UserService = userService;
        AuthService = authService;
        SystemSettingService = systemSettingService;
    }

    public async Task<Token> Handle(UserRefreshTokenQuery request,
        CancellationToken cancellationToken)
    {
        DateTime? TokenExpireDate = AuthService.GetTokenExpireDate();

        if (TokenExpireDate == null)
        {
            throw new ForbiddenAccessException();
        }

        SystemSetting systemSetting =await SystemSettingService.GetSystemSetting();
         if (TokenExpireDate?.AddSeconds(systemSetting.RefreshTokenExpireSeconds)<DateTime.Now)
        {
            throw new ForbiddenAccessException();
        }
       
        Guid UserID = AuthService.GetUserGUID();
        
        var user = UserService.Table.FirstOrDefault(a => a.GUID==UserID);

        List<long> UserRoleGkeys = AuthService.GetUserRoleGkeys();
        List<UserRole> UserRoles = UserService.db.UserRole.Include(a=>a.Role)
            .Where(a =>a.UserGUID==UserID &&a.ExpireDate>=DateTime.Now && UserRoleGkeys.Any(b => b == a.Gkey)).ToList();
        
        var key = await UserService.CreateToken(user, UserRoles,false);

        var token = new Token()
        {
            ForceChangePassword = user.ForceChangePassword,
            Key = key.Token,
            MultiLogin = key.MultiLogin,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ExpireDate = key.ExpireDate,
            UserRole = UserRoles.OrderBy(a => a.Role.Level).ThenBy(a => a.Gkey).First().Gkey
        };
        return token;

     
    }
}