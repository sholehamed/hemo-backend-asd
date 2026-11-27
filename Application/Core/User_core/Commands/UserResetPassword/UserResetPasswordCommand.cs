using Application.Common.Utility.Security;

namespace Application.Core.User_core.Commands.UserResetPassword;

public record UserResetPasswordCommand : IRequest<ResetPasswordResult>, IMapFrom<User>
{
    [PropConfig("رمز عبور")] public string Password { get; init; }
    [PropConfig("رمز عبور فعلی")] public string OldPassword { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserResetPasswordCommand, User>();
    }
}
public record ResetPasswordResult : IMapFrom<User>
{
    [PropConfig("نام کاربری", 16)] public string Username { get; init; }

    [PropConfig("نام")] public string FirstName { get; init; }

    [PropConfig("نام خانوادگی")] public string LastName { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, ResetPasswordResult>()
            ;
    }
}

public class UserResetPasswordCommandHandler : IRequestHandler<UserResetPasswordCommand,ResetPasswordResult>
{
    private UserService UserService;
    private AuthService AuthService;

    public UserResetPasswordCommandHandler(UserService userService, AuthService authService)
    {
        UserService = userService;
        AuthService = authService;
    }

    public async Task<ResetPasswordResult> Handle(UserResetPasswordCommand request, CancellationToken cancellationToken)
    {
        Guid UserGUID = AuthService.GetUserGUID();
        await UserService.ChangePassword(UserGUID, request.OldPassword, request.Password, true);
        var Data = UserService.Table.AsNoTracking().Where(a => a.GUID == UserGUID)
            .ProjectTo<ResetPasswordResult>(UserService.mapper.ConfigurationProvider).FirstOrDefault();
        return Data;
    }
}