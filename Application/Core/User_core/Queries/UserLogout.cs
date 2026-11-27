using Application.Common.Utility.Security;

namespace Application.Core.User_core.Queries;

public record UserLogoutQuery : IRequest<UserLogoutDTO>;

public record UserLogoutDTO : IMapFrom<User>
{
    [PropConfig("نام کاربری", MaxLenght: 64)]
    public string? Username { get; init; }

    [PropConfig("نام")] public string FirstName { get; init; }

    [PropConfig("نام خانوادگی")] public string LastName { get; init; }

    [PropConfig("موبایل", MaxLenght: 16)] public string? Mobile { get; init; }

    [PropConfig("ایمیل", MaxLenght: 64)] public string? Email { get; init; }

    [PropConfig("کد ملی", MaxLenght: 10)] public string? Nationalcode { get; init; }


    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserLogoutDTO>();
    }
}

public class
    UserLogoutQueryHandler : IRequestHandler<UserLogoutQuery, UserLogoutDTO>
{
    private UserService UserService;
    private AuthService AuthService;

    public UserLogoutQueryHandler(UserService userService, AuthService authService)
    {
        UserService = userService;
        AuthService = authService;
    }

    public async Task<UserLogoutDTO> Handle(UserLogoutQuery request,
        CancellationToken cancellationToken)
    {
        Guid UserGUID = AuthService.GetUserGUID();
        UserService.Logout(UserGUID,true);
        UserLogoutDTO? Data = UserService.Table.AsNoTracking().Where(a => a.GUID == UserGUID)
            .ProjectTo<UserLogoutDTO>(UserService.mapper.ConfigurationProvider).FirstOrDefault();

        return Data;
    }
}