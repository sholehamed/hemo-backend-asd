

using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Core.UserRole_core.Queries;

public record UserRoleFindAllQuery : IRequest<List<UserRoleFindAllDTO>>
{

}

public record UserRoleFindAllDTO : IMapFrom<UserRole>
{

    [PropConfig("کاربر")]
    public Guid UserGUID { get; init; }

    [PropConfig("نقش")]
    public Guid RoleGUID { get; init; }

    [PropConfig("شرکت")]
    public Guid? OrgLocationGUID { get; init; }

    [PropConfig("تاریخ انقضا")]
    public DateTime ExpireDate { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserRole, UserRoleFindAllDTO>();
    }
}

public class
    UserRoleFindAllQueryHandler : IRequestHandler<UserRoleFindAllQuery,
    List<UserRoleFindAllDTO>>
{
    private UserRoleService UserRoleService;
    private OrgLocationService OrgLocationService;
    public UserRoleFindAllQueryHandler(UserRoleService UserRoleService,
        OrgLocationService OrgLocationService)
    {
        this.UserRoleService = UserRoleService;
        this.OrgLocationService = OrgLocationService;
    }

    public async Task<List<UserRoleFindAllDTO>> Handle(UserRoleFindAllQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        List<Guid> Childs = OrgLocationService.GetChilds(userRoleInfo.OrgLocationGUID).Result;
        return await UserRoleService.Table
                        .Where(s => s.OrgLocationGUID == userRoleInfo.OrgLocationGUID ||
                        Childs.Contains(s.OrgLocationGUID))
                        .AsNoTracking()
            .ProjectTo<UserRoleFindAllDTO>(UserRoleService.mapper.ConfigurationProvider).ToListAsync();
    }
}



