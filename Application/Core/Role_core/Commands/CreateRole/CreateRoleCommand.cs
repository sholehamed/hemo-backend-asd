using Application.Common.Models;

namespace Application.Core.Role_core.Commands.CreateRole;

public record CreateRoleCommand : IRequest<Guid>, IMapFrom<Role>
{
    [PropConfig("بالا دست")] public Guid? PRoleGUID { get; init; }

    [PropConfig("عنوان")] public string Name { get; init; }
    [PropConfig("نوع شرکت")] public Guid? OrgLocationTypeGUIO { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateRoleCommand, Role>();
    }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly RoleService RoleService;
    private UserRoleService UserRoleService;

    public CreateRoleCommandHandler(RoleService roleService, UserRoleService userRoleService)
    {
        RoleService = roleService;
        UserRoleService = userRoleService;
    }

    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.PRoleGUID.HasValue)
        {
            await RoleService.RoleIsValid(request.PRoleGUID);
        }

        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();

        var entity = RoleService.mapper.Map<Role>(request);

        if (!request.PRoleGUID.HasValue)
        {
            entity.PRoleGUID = userRoleInfo.RoleGuid;
        }

        if (!userRoleInfo.IsSuperAdmin)
        {
            entity.OrgLocationTypeGUID = null;
        }


        entity.OrgLocationGUID = userRoleInfo.OrgLocationGUID;
        RoleService.Table.Add(entity);

        await RoleService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}