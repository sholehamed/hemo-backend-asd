
using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Core.UserRole_core.Commands.UpdateUserRole;

public record UpdateUserRoleCommand : IRequest, IMapFrom<UserRole>
{
 
   [PropConfig("کاربر")]
   public Guid UserGUID { get; init; }
 
   [PropConfig("نقش")]
   public Guid RoleGUID { get; init; }
 
   [PropConfig("تاریخ انقضا")]
   public DateTime ExpireDate { get; init; }
   
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    [PropConfig("محل سازمان")]
    public Guid OrgLocationGUID { get; init; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateUserRoleCommand, UserRole>();
    }
}

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand>
{
    private UserRoleService UserRoleService;
    private OrgLocationService OrgLocationService;
    private RoleService RoleService;

    public UpdateUserRoleCommandHandler(UserRoleService userRoleService, OrgLocationService orgLocationService, RoleService roleService)
    {
        UserRoleService = userRoleService;
        OrgLocationService = orgLocationService;
        RoleService = roleService;
    }

    public async Task Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        //await RoleService.RoleIsValid(request.RoleGUID);

        UserRole? OldEntity = await UserRoleService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        await RoleService.RoleIsValid(OldEntity.RoleGUID);
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        bool CheckHasAccessToChild = await OrgLocationService.HasAccessToChild(userRoleInfo.OrgLocationGUID, request.OrgLocationGUID);
        if (!CheckHasAccessToChild)
        {
            throw new ServiceExeption("دسترسی غیرمجاز اطلاعات");
        }

        if (OldEntity.OrgLocationGUID != userRoleInfo.OrgLocationGUID && !userRoleInfo.IsSuperAdmin)
        {
            throw new ServiceExeption("خطای امنیتی رخ داده است");
        }

        // بررسی زیر مجموعه های فاتحان
        if (!userRoleInfo.IsSuperAdmin && UserRoleService.db.OrgLocation.Any(hh => hh.GUID == request.OrgLocationGUID && hh.Path.StartsWith("/1/6/7/")))
        {
            throw new ServiceExeption("امکان ایجاد دسترسی برای زیر مجموعه های فاتحان عصر تدبیر امکان پذیر نیست!");
        }

        UserRoleService.mapper.Map(request, OldEntity);
        
        await UserRoleService.SaveChangesAsync(cancellationToken);
    }
}

