using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;
using Tool_Utility.Encryption;

namespace Application.Core.sso_core.Commands.ssoSsoLogin;

public record ssoSsoLoginCommand : IRequest<Token>, IMapFrom<sso>
{
    [PropConfig("کلید")] public string Key { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ssoSsoLoginCommand, sso>();
    }
}

public class ssoSsoLoginCommandHandler : IRequestHandler<ssoSsoLoginCommand, Token>
{
    private ssoService ssoService;
    private AuthService AuthService;
    private UserService UserService;

    public ssoSsoLoginCommandHandler(ssoService ssoService, AuthService authService, UserService userService)
    {
        this.ssoService = ssoService;
        AuthService = authService;
        UserService = userService;
    }

    public async Task<Token> Handle(ssoSsoLoginCommand request, CancellationToken cancellationToken)
    {
        Token token = new Token();
        sso? sso = null;
        try
        {
            string ErrorMessage = "توکن احراز هویت معتبر نمی باشد.";

            List<string> Keys = request.Key.Split('@').ToList();

            sso = ssoService.Table.FirstOrDefault(a => a.Key == Keys[0]);
            if (sso == null)
            {
                throw new ServiceExeption(ErrorMessage);
            }

            if (sso.ExpireDate < DateTime.Now)
            {
                throw new ServiceExeption(ErrorMessage);
            }

            //if (sso.IPAddress != AuthService.GetIpAddress())
            //{
            //    throw new ServiceExeption(ErrorMessage);
            //}

            if (sso.UserAgent != AuthService.GetUserAgent())
            {
                throw new ServiceExeption(ErrorMessage);
            }

            string Sign =
                Encryption.SHA256Hash(Keys[0] + Keys[1] + "hLl8GOipd5qL7ZxHmI5hNIznBIHXcq0GIMb5dLnK1DUaFMnr8T");

            if (sso.Sign != Sign)
            {
                throw new ServiceExeption(ErrorMessage);
            }

            User user = ssoService.db.User.First(a => a.GUID == sso.UserGUID);

            token = await UserService.GetToken(user, sso.PackageGUID, sso.OrgLocationGUID,sso.AdminLogin,MultiLoginAlowed:true
                );
        }
        catch (Exception e)
        {
            throw e;
        }
        finally
        {
            if (sso != null)
            {
                ssoService.Table.Remove(sso);
                await ssoService.SaveChangesAsync(cancellationToken);
            }
        }

        return token;
    }
}