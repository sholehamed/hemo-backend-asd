using Application.Common.Models;
using System;

namespace Application.Core.User_core.Queries;

public record UserWithPaginationQuery : PageFilter<User, UserWithPaginationDTO>
{
}

public record UserWithPaginationDTO : IMapFrom<User>
{
    [PropConfig("نام کاربری", 16)] public string Username { get; init; }
    [PropConfig("نام")] public string FirstName { get; init; }

    [PropConfig("نام خانوادگی")] public string LastName { get; init; }

    [PropConfig("موبایل", 16)] public string? Mobile { get; init; }

    [PropConfig("ایمیل", 64)] public string? Email { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }
    [PropConfig("کد ملی", MaxLenght: 10)]
    public string? Nationalcode { get; set; }
    
    [PropConfig("حساب کاربری مسدود شده است")]
    public bool Blocked { get; set; }
        
    [PropConfig("حساب کاربری قفل شده است")]
    public bool Locked { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserWithPaginationDTO>();
    }
}

public class
    UserWithPaginationQueryHandler : IRequestHandler<UserWithPaginationQuery,
    PaginatedList<UserWithPaginationDTO>>
{
    private readonly UserService UserService;
    private UserRoleService UserRoleService;

    public UserWithPaginationQueryHandler(UserService userService, UserRoleService userRoleService)
    {
        UserService = userService;
        UserRoleService = userRoleService;
    }

    public async Task<PaginatedList<UserWithPaginationDTO>> Handle(UserWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        IQueryable<User> queryable = UserService.Table.Where(a =>
            userRoleInfo.IsSuperAdmin == true ||
            a.UserRoles.Any(x => 
                                x.OrgLocation.Path.StartsWith(userRoleInfo.OrgLocationPath)
                            ) ||
            a.UserInvites.Any(
                b => b.UserAccepted == true && b.OrgLocation.Path!="/1/" && userRoleInfo.OrgLocationPath.StartsWith(b.OrgLocation.Path))
        );
        return await request.ToPaging(queryable, UserService.mapper);
    }
}