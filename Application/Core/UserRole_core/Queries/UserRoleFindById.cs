
using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Core.UserRole_core.Queries;

public record UserRoleFindByIdQuery(Guid GUID) : IRequest<UserRoleFindByIdDTO>;

public record UserRoleFindByIdDTO : IMapFrom<UserRole>
{
    
    [PropConfig("کاربر")]
    public Guid UserGUID { get; init; }
    
    [PropConfig("نقش")]
    public Guid RoleGUID { get; init; }
    
    [PropConfig("سازمان")]
    public Guid? OrganizationGUID { get; init; }
    
    [PropConfig("شرکت")]
    public Guid? OrgLocationGUID { get; init; }

    [PropConfig("شرکت")]
    public string? OrgLocationName { get; init; }

    [PropConfig("تاریخ انقضا")]
    public DateTime ExpireDate { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserRole, UserRoleFindByIdDTO>()
            .ForMember(a => a.OrgLocationName, opt => opt.MapFrom(s => s.OrgLocation.Name))
            ;
    }
}

public class
    UserRoleFindByIdQueryHandler : IRequestHandler<UserRoleFindByIdQuery, UserRoleFindByIdDTO>
{
    private UserRoleService UserRoleService;
    private OrgLocationService OrgLocationService;
    private RoleService RoleService;

    public UserRoleFindByIdQueryHandler(UserRoleService userRoleService, OrgLocationService orgLocationService, RoleService roleService)
    {
        UserRoleService = userRoleService;
        OrgLocationService = orgLocationService;
        RoleService = roleService;
    }

    public async Task<UserRoleFindByIdDTO> Handle(UserRoleFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleFindByIdDTO? Data = UserRoleService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<UserRoleFindByIdDTO>(UserRoleService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        
        await OrgLocationService.OrgLocationIsValid(Data.OrgLocationGUID,true);
        await RoleService.RoleIsValid(Data.RoleGUID);
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        bool CheckHasAccessToChild = await OrgLocationService.HasAccessToChild(userRoleInfo.OrgLocationGUID, Data.OrgLocationGUID.Value);
        if (!CheckHasAccessToChild)
        {
            throw new ServiceExeption("دسترسی غیرمجاز اطلاعات");
        }
        return Data;
    }
}

