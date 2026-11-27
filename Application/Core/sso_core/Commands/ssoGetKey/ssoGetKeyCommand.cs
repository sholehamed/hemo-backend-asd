using Application.Common.Exceptions;
using Application.Common.Utility.Security;
using Domain.Entities;
using Tool_Utility.Encryption;

namespace Application.Core.sso_core.Commands.ssoGetKey;

public record ssoGetKeyCommand : IRequest<string>, IMapFrom<sso>
{
    [PropConfig("پکیج")] public Guid PackageGUID { get; init; }
    [PropConfig("شرکت")] public Guid OrgLocationGUID { get; init; }

    [PropConfig("لاگین به مدیریت کاربران")]
    public bool AdminLogin { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ssoGetKeyCommand, sso>();
    }
}

public class ssoGetKeyCommandHandler : IRequestHandler<ssoGetKeyCommand, string>
{
    private AuthService AuthService;
    private ssoService ssoService;
    MenuService MenuService;

    public ssoGetKeyCommandHandler(AuthService authService, ssoService ssoService, MenuService menuService)
    {
        AuthService = authService;
        this.ssoService = ssoService;
        MenuService = menuService;
    }

    public async Task<string> Handle(ssoGetKeyCommand request, CancellationToken cancellationToken)
    {
        Guid UserID = AuthService.GetUserGUID();
        if (UserID != Guid.Empty)
        {
            DateTime NowDate = DateTime.Now;
            bool PackgeIsActive = ssoService.db.OrglocationPackage.Any(a =>
                a.ExpiryDate > NowDate &&
                a.OrgLocationGUID == request.OrgLocationGUID && a.PackageGUID == request.PackageGUID);

            if (!PackgeIsActive)
            {
                throw new ServiceExeption("اعتبار پکیج به اتمام رسیده است.");
            }

            Role packageRole = ssoService.db.Package.Where(a => a.GUID == request.PackageGUID)
                .Select(a => a.Role).First();

            var package = ssoService.db.Package.Include(hh => hh.MainMenu).First(a => a.GUID == request.PackageGUID);
//            string IdentityPath = await MenuService.GetIdentityPath();

            bool UserAcessIsValid = ssoService.db.UserRole.Any(a =>
                a.UserGUID == UserID &&
                a.OrgLocationGUID == request.OrgLocationGUID &&
                a.ExpireDate > NowDate &&
                (
                    a.Role.GUID == packageRole.GUID ||
                    a.Role.RoleAccess.Any(RoleAccess => package.MainMenuGuid != null && RoleAccess.Menu.Path.StartsWith(package.MainMenu.Path)
                    //ssoService.db.RoleAccess.Any(packageRoleAccess =>
                    //    //!packageRoleAccess.Menu.Path.Contains(IdentityPath) &&
                    //    packageRoleAccess.RoleGUID == packageRole.GUID &&
                    //    RoleAccess.MenuGUID == packageRoleAccess.MenuGUID)
                    ) ||
                    a.Role.Gkey == 1
                )
            );

            if (!UserAcessIsValid)
            {
                throw new ServiceExeption("شما به این پکیج دسترسی ندارید.");
            }

            await ssoService.Table
                .Where(u => u.UserGUID == UserID)
                .ExecuteDeleteAsync(cancellationToken);

            string Sign =
                (Guid.NewGuid().ToString() + Guid.NewGuid().ToString()).Replace("-", "");

            sso entity = new sso()
            {
                UserGUID = UserID,
                PackageGUID = request.PackageGUID,
                IPAddress = AuthService.GetIpAddressFromToken(),
                UserAgent = AuthService.GetUserAgentFromToken(),
                ExpireDate = DateTime.Now.AddSeconds(60),
                OrgLocationGUID = request.OrgLocationGUID,
                AdminLogin = request.AdminLogin,
                Key = (Guid.NewGuid().ToString() + Guid.NewGuid().ToString() + Guid.NewGuid().ToString() +
                       Guid.NewGuid().ToString()).Replace("-", ""),
            };

            entity.Sign =
                Encryption.SHA256Hash(entity.Key + Sign + "hLl8GOipd5qL7ZxHmI5hNIznBIHXcq0GIMb5dLnK1DUaFMnr8T");

            ssoService.Table.Add(entity);

            await ssoService.SaveChangesAsync(cancellationToken);

            return entity.Key + "@" + Sign;
        }
        else
        {
            throw new UnauthorizedAccessException();
        }
    }
}