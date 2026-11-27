using Application.Common.Exceptions;
using Application.Common.Models;

namespace Application.Core.Role_core.Commands.UpdateRole;

public record UpdateRoleCommand : IRequest, IMapFrom<Role>
{
    [PropConfig("نوع شرکت")] public Guid? OrgTypeGUID { get; set; }
    [PropConfig("عنوان")] public string Name { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }
    [PropConfig("نوع شرکت")] public Guid? OrgLocationTypeGUIO { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateRoleCommand, Role>();
    }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand>
{
    private readonly RoleService RoleService;
    private UserRoleService UserRoleService;

    public UpdateRoleCommandHandler(RoleService roleService, UserRoleService userRoleService)
    {
        RoleService = roleService;
        UserRoleService = userRoleService;
    }

    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        await RoleService.RoleIsValid(request.GUID);

        var OldEntity = await RoleService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();

        if (OldEntity.OrgLocationGUID != userRoleInfo.OrgLocationGUID)
        {
            throw new ServiceExeption("خطای امنیتی رخ داده است");
        }


        Guid? OrgLocationTypeGUIO = OldEntity.OrgLocationTypeGUID;

        RoleService.mapper.Map(request, OldEntity);

        if (!userRoleInfo.IsSuperAdmin)
        {
            OldEntity.OrgLocationTypeGUID = OrgLocationTypeGUIO;
        }

        await RoleService.SaveChangesAsync(cancellationToken);
    }
}