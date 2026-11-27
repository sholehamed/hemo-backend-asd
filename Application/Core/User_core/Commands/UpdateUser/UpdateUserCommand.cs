namespace Application.Core.User_core.Commands.UpdateUser;

public record UpdateUserCommand : IRequest, IMapFrom<User>
{
    [PropConfig("نام کاربری", 16)] public string Username { get; init; }

    [PropConfig("نام")] public string FirstName { get; init; }

    [PropConfig("نام خانوادگی")] public string LastName { get; init; }
    [PropConfig("موبایل", 16)] public string? Mobile { get; init; }

    [PropConfig("ایمیل", 64)] public string? Email { get; init; }
    [PropConfig("رمز عبور")] public string Password2 { get; set; }
    [PropConfig("کلید")] public Guid GUID { get; init; }
    
    [PropConfig("کد ملی", MaxLenght: 10)]
    public string? Nationalcode { get; set; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateUserCommand, User>();
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly UserService UserService;

    public UpdateUserCommandHandler(UserService UserService)
    {
        this.UserService = UserService;
    }

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var OldEntity = await UserService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        UserService.mapper.Map(request, OldEntity);

        if (!request.Password2.Contains("*******"))
        {
            OldEntity.Password = UserService.HashPassword(OldEntity.GUID, request.Password2);
            OldEntity.ForceChangePassword = true;
        }

        await UserService.SaveChangesAsync(cancellationToken);
    }
}