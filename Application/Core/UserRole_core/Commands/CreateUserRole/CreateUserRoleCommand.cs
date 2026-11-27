using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Core.UserRole_core.Commands.CreateUserRole;

public record CreateUserRoleCommand : IRequest<Guid>, IMapFrom<UserRole>
{
    [PropConfig("کاربر")] public Guid UserGUID { get; init; }
    [PropConfig("نقش")] public Guid RoleGUID { get; init; }
    [PropConfig("تاریخ انقضا")] public DateTime ExpireDate { get; init; }
    [PropConfig("محل سازمان")] public Guid? OrgLocationGUID { get; set; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateUserRoleCommand, UserRole>();
    }
}

public class CreateUserRoleCommandHandler : IRequestHandler<CreateUserRoleCommand, Guid>
{
    private UserRoleService UserRoleService;
    private OrgLocationService OrgLocationService;
    private RoleService RoleService;

    public CreateUserRoleCommandHandler(UserRoleService userRoleService, OrgLocationService orgLocationService,
        RoleService roleService)
    {
        UserRoleService = userRoleService;
        OrgLocationService = orgLocationService;
        RoleService = roleService;
    }

    public async Task<Guid> Handle(CreateUserRoleCommand request, CancellationToken cancellationToken)
    {
        //await RoleService.RoleIsValid(request.RoleGUID);
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        UserRole entity = UserRoleService.mapper.Map<UserRole>(request);
        if(request.OrgLocationGUID is null)
        {
            entity.OrgLocationGUID = userRoleInfo.OrgLocationGUID;
        }
        bool CheckHasAccessToChild = await OrgLocationService.HasAccessToChild(userRoleInfo.OrgLocationGUID, entity.OrgLocationGUID);
        if (!CheckHasAccessToChild)
        {
            throw new ServiceExeption("دسترسی غیرمجاز اطلاعات");
        }

        // بررسی زیر مجموعه های فاتحان
        if (
            !userRoleInfo.IsSuperAdmin &&
            UserRoleService.db.OrgLocation.Any(hh => hh.GUID == entity.OrgLocationGUID && hh.UserGUID != userRoleInfo.UserGuid && hh.Path.StartsWith("/1/6/7/"))){
            throw new ServiceExeption("امکان ایجاد دسترسی برای زیر مجموعه های فاتحان عصر تدبیر امکان پذیر نیست!");
        }

        UserRoleService.Table.Add(entity);

        await UserRoleService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}