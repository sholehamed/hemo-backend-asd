namespace Application.Core.User_core.Commands.UserLockUser;

public record UserLockUserCommand : IRequest, IMapFrom<User>
{
    [PropConfig("حساب کاربری قفل شده است")]
    public bool Locked { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserLockUserCommand, User>();
    }
}

public class UserLockUserCommandHandler : IRequestHandler<UserLockUserCommand>
{
    private UserService UserService;

    public UserLockUserCommandHandler(UserService UserService)
    {
        this.UserService = UserService;
    }

    public async Task Handle(UserLockUserCommand request, CancellationToken cancellationToken)
    {
        User? OldEntity = await UserService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        UserService.mapper.Map(request, OldEntity);

        await UserService.SaveChangesAsync(cancellationToken);
    }
}