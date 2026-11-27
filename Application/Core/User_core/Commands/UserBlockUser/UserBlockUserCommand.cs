namespace Application.Core.User_core.Commands.UserBlockUser;

public record UserBlockUserCommand : IRequest, IMapFrom<User>
{
    [PropConfig("حساب کاربری مسدود شده است")]
    public bool Blocked { get; init; }
    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserBlockUserCommand, User>();
    }
}

public class UserBlockUserCommandHandler : IRequestHandler<UserBlockUserCommand>
{
    private UserService UserService;

    public UserBlockUserCommandHandler(UserService UserService)
    {
        this.UserService = UserService;
    }

    public async Task Handle(UserBlockUserCommand request, CancellationToken cancellationToken)
    {
        User? OldEntity = await UserService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        UserService.mapper.Map(request, OldEntity);

        if (OldEntity.Blocked == true)
        {
            UserService.Logout(request.GUID,true);
        }
        
        await UserService.SaveChangesAsync(cancellationToken);
    }
}