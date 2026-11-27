using Application.Common.Exceptions;
using Application.Common.Models;
using Tool_Utility.Encryption;

namespace Application.Core.User_core.Commands.UserNotificationLogin;

public record UserNotificationLoginCommand : IRequest<Token>, IMapFrom<User>
{
    [PropConfig("نام کاربری", 16)] public string Username { get; init; }
    [PropConfig("رمز عبور")] public string Password { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserNotificationLoginCommand, User>();
    }
}

public class UserNotificationLoginCommandHandler : IRequestHandler<UserNotificationLoginCommand, Token>
{
    private UserService UserService;

    public UserNotificationLoginCommandHandler(UserService UserService)
    {
        this.UserService = UserService;
    }

    public async Task<Token> Handle(UserNotificationLoginCommand request, CancellationToken cancellationToken)
    {
        var username = request.Username?.Trim().ToLower();

        var user = UserService.db.User
            .FirstOrDefault(a => a.Username.ToLower() == username);
        
        if (user == null || !PasswordIsValid(user, request.Password))
        {
            var errorMessage = "نام کاربری یا رمز عبور اشتباه می باشد.";
            throw new ServiceExeption(errorMessage);
        }

        Token Token = new Token()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Key = Encryption.base64Encrypt(user.GUID.ToString())
        };
        return Token;
    }
    
    public bool PasswordIsValid(User user, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return user.Password ==
               UserService.HashPassword(user.GUID, password);
    }
}