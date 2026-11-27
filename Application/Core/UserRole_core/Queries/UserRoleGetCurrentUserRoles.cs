using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Core.UserRole_core.Queries;

public record UserRoleGetCurrentUserRolesQuery : IRequest<List<UserRoleGetCurrentUserRolesDTO>>
{
}

public record UserRoleGetCurrentUserRolesDTO : IMapFrom<UserRole>
{
    [PropConfig("عنوان نقش")] public string Name { get; init; }
    [PropConfig("کد")] public long Gkey { get; init; }
    [PropConfig("شرکت")] public string OrgLocationName { get; set; }
    [PropConfig("سازمان")] public string OrganizationName { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserRole, UserRoleGetCurrentUserRolesDTO>()
            .ForMember(dest => dest.OrgLocationName, opt => opt.MapFrom(src => src.OrgLocation.Name))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Role.Name + " (" + src.OrgLocation.Name + ")"))
            ;
    }
}

public class
    UserRoleGetCurrentUserRolesQueryHandler : IRequestHandler<UserRoleGetCurrentUserRolesQuery,
    List<UserRoleGetCurrentUserRolesDTO>>
{
    private UserRoleService UserRoleService;
    AuthService AuthService;
    private OrgLocationService OrgLocationService;

    public UserRoleGetCurrentUserRolesQueryHandler(UserRoleService userRoleService, AuthService authService,
        OrgLocationService OrgLocationService)
    {
        UserRoleService = userRoleService;
        AuthService = authService;
        this.OrgLocationService = OrgLocationService;
    }

    public async Task<List<UserRoleGetCurrentUserRolesDTO>> Handle(UserRoleGetCurrentUserRolesQuery request,
        CancellationToken cancellationToken)
    {
        //UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        //Guid CurrentOrgLocationGuid = userRoleInfo.OrgLocationGUID;
        //List<Guid> Childs = OrgLocationService.GetChilds(CurrentOrgLocationGuid).Result;

        List<long> UserRoles = AuthService.GetUserRoleGkeys();
        return await UserRoleService.Table.AsNoTracking().Where(A => UserRoles.Contains(A.Gkey) 
        //&& (A.OrgLocationGUID == CurrentOrgLocationGuid || Childs.Contains(A.OrgLocationGUID))
        )
            .ProjectTo<UserRoleGetCurrentUserRolesDTO>(UserRoleService.mapper.ConfigurationProvider).ToListAsync();
    }
}