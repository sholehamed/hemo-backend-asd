namespace Application.Core.User_core.Commands.CreateUser;

public record CreateUserCommand : IRequest<Guid>, IMapFrom<User>
{
    [PropConfig("نام کاربری", 16)] public string Username { get; init; }
    [PropConfig("رمز عبور")] public string Password { get; init; }
    [PropConfig("نام")] public string FirstName { get; init; }
    [PropConfig("نام خانوادگی")] public string LastName { get; init; }
    [PropConfig("موبایل", 16)] public string? Mobile { get; init; }
    [PropConfig("ایمیل", 64)] public string? Email { get; init; }
    [PropConfig("کد ملی", MaxLenght: 10)]
    public string? Nationalcode { get; set; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateUserCommand, User>();
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly UserService UserService;

    public CreateUserCommandHandler(UserService UserService)
    {
        this.UserService = UserService;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = UserService.mapper.Map<User>(request);
        return (await UserService.CreateUser(entity.Username, entity.Password, entity.FirstName, entity.LastName,
            entity.Email, entity.Mobile, Guid.Parse("0A5E5228-9C6C-4B81-BCA4-9BE6363C7AAE"),
            $"پروفایل شخصی {entity.FirstName} {entity.LastName}", 
            "", "", "", "", "", DateTime.Now.AddYears(100),
            true,request.Nationalcode,
            cancellationToken)).UserGUID;
    }
}