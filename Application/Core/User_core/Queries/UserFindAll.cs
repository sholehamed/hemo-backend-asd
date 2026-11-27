using Application.Common.Models;

namespace Application.Core.User_core.Queries;

public record UserFindAllQuery : IRequest<List<UserFindAllDTO>>
{
}

public record UserFindAllDTO : IMapFrom<User>
{
    [PropConfig("نام کاربری", 16)] public string Username { get; init; }

    [PropConfig("نام")] public string FirstName { get; init; }

    [PropConfig("نام خانوادگی")] public string LastName { get; init; }

    [PropConfig("موبایل", 16)] public string? Mobile { get; init; }

    [PropConfig("ایمیل", 64)] public string? Email { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserFindAllDTO>();
    }
}

public class
    UserFindAllQueryHandler : IRequestHandler<UserFindAllQuery,
    List<UserFindAllDTO>>
{
    private readonly UserService UserService;
    UserRoleService UserRoleService;

    public UserFindAllQueryHandler(UserService UserService, UserRoleService userRoleService)
    {
        this.UserService = UserService;
        UserRoleService = userRoleService;
    }

    public async Task<List<UserFindAllDTO>> Handle(UserFindAllQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        return await UserService.Table.AsNoTracking().Where(a => a.GUID == userRoleInfo.UserGuid || a.UserInvites.Any(b => b.OrgLocationGUID == userRoleInfo.OrgLocationGUID && b.UserAccepted == true && b.OrgLocationAccepted == true))
            .ProjectTo<UserFindAllDTO>(UserService.mapper.ConfigurationProvider).ToListAsync();
    }
}