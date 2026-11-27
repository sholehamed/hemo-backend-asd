using Application.Common.Models;

namespace Application.Core.User_core.Queries;

public record UserOnlineUsersQuery : PageFilter<User, UserOnlineUsersDTO>
{
}

public record UserOnlineUsersDTO : IMapFrom<User>
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
        profile.CreateMap<User, UserOnlineUsersDTO>();
    }
}

public class
    UserOnlineUsersQueryHandler : IRequestHandler<UserOnlineUsersQuery,
    PaginatedList<UserOnlineUsersDTO>>
{
    private UserService UserService;

    public UserOnlineUsersQueryHandler(UserService UserService)
    {
        this.UserService = UserService;
    }

    public async Task<PaginatedList<UserOnlineUsersDTO>> Handle(UserOnlineUsersQuery request,
        CancellationToken cancellationToken)
    {
        HashSet<Guid> OnlineUsers = new HashSet<Guid>();
      await  UserService._lock.WaitAsync();
        try
        {
            OnlineUsers = UserService._userTokens.Where(a => a.ExpireDate > DateTime.Now).Select(a => a.UserGUID)
                .ToHashSet();
        }
       
        finally
        {
            UserService._lock.Release();
        }

        IQueryable<User> queryable = UserService.Table.Where(a => OnlineUsers.Contains(a.GUID)
        );
        return await request.ToPaging(queryable, UserService.mapper);
    }
}