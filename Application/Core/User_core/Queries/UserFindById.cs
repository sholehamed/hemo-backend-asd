namespace Application.Core.User_core.Queries;

public record UserFindByIdQuery(Guid GUID) : IRequest<UserFindByIdDTO>;

public record UserFindByIdDTO : IMapFrom<User>
{
    [PropConfig("نام کاربری", 16)] public string Username { get; init; }

    [PropConfig("نام")] public string FirstName { get; init; }

    [PropConfig("نام خانوادگی")] public string LastName { get; init; }

    [PropConfig("موبایل", 16)] public string? Mobile { get; init; }

    [PropConfig("ایمیل", 64)] public string? Email { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }
    [PropConfig("رمز عبور")] public string Password2 { get; set; }

    [PropConfig("کد ملی", MaxLenght: 10)]
    public string? Nationalcode { get; set; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserFindByIdDTO>()
            .ForMember(dest => dest.Password2, opt => opt.MapFrom(src => "*********"))
            ;
    }
}

public class
    UserFindByIdQueryHandler : IRequestHandler<UserFindByIdQuery, UserFindByIdDTO>
{
    private readonly UserService UserService;

    public UserFindByIdQueryHandler(UserService UserService)
    {
        this.UserService = UserService;
    }

    public async Task<UserFindByIdDTO> Handle(UserFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        var Data = UserService.Table.AsNoTracking().Where(a => a.GUID == request.GUID)
            .ProjectTo<UserFindByIdDTO>(UserService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}