using Application.Common.Exceptions;
using Application.Common.Models;
using Google.Apis.Auth;

namespace Application.Core.User_core.Commands.UserGoogleLogin;

public record UserGoogleLoginCommand : IRequest<Token>, IMapFrom<User>
{
    [PropConfig(" توکن", MaxLenght: 16)] public string credential { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserGoogleLoginCommand, User>();
    }
}

public class UserGoogleLoginCommandHandler : IRequestHandler<UserGoogleLoginCommand, Token>
{
    private UserService UserService;

    public UserGoogleLoginCommandHandler(UserService UserService)
    {
        this.UserService = UserService;
    }

    public async Task<Token> Handle(UserGoogleLoginCommand request, CancellationToken cancellationToken)
    {
        throw new ServiceExeption("ورود با گوگل غیر فعال می باشد.");
        var payload = await GoogleJsonWebSignature.ValidateAsync(
            request.credential,
            new GoogleJsonWebSignature.ValidationSettings()
            {
                IssuedAtClockTolerance = TimeSpan.FromMinutes(120)
            }
        );

        if (Environment.GetEnvironmentVariable("Audience") == payload.Audience.ToString())
        {
            User? User = UserService.Table.AsNoTracking().FirstOrDefault(a => a.Email == payload.Email);
            if (User == null)
            {
                User = new User()
                {
                    Email = payload.Email,
                    FirstName = payload.Name,
                    LastName = payload.FamilyName,
                    Mobile = null,
                    Username = null,
                };
                //  await UserService.CreateUser(User, cancellationToken);
            }

            return await UserService.GetToken(User, null);
        }
        else
        {
            throw new ServiceExeption("خطای امنیتی رخ داده است.");
        }
    }
}