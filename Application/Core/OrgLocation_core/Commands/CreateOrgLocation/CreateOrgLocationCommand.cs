using Application.Common.Models;

namespace Application.Core.OrgLocation_core.Commands.CreateOrgLocation;

public record CreateOrgLocationCommand : IRequest<Guid>, IMapFrom<OrgLocation>
{
    [PropConfig("عنوان")] public string Name { get; init; }

    [PropConfig("شرکت پدر")] public Guid? POrgLocationGUID { get; init; }

    [PropConfig("آدرس")] public string? Address { get; init; }

    [PropConfig("تلفن")] public string? Tell { get; init; }

    [PropConfig("فکس")] public string? Fax { get; init; }

    [PropConfig("ایمیل")] public string? Email { get; init; }

    [PropConfig("آدرس وبسایت")] public string? Url { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateOrgLocationCommand, OrgLocation>();
    }
}

public class CreateOrgLocationCommandHandler : IRequestHandler<CreateOrgLocationCommand, Guid>
{
    private OrgLocationService OrgLocationService;
    private UserRoleService UserRoleService;

    public CreateOrgLocationCommandHandler(OrgLocationService orgLocationService, UserRoleService userRoleService)
    {
        OrgLocationService = orgLocationService;
        UserRoleService = userRoleService;
    }

    public async Task<Guid> Handle(CreateOrgLocationCommand request, CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();

        OrgLocation entity = OrgLocationService.mapper.Map<OrgLocation>(request);
        entity.UserGUID = userRoleInfo.UserGuid;
        if (entity.POrgLocationGUID == null)
        {
            entity.POrgLocationGUID = userRoleInfo.OrgLocationGUID;
        }
        else
        {
            await OrgLocationService.OrgLocationIsValid(request.POrgLocationGUID);
            entity.OrgLocationTypeGUIO = OrgLocationService.db.OrgLocation.Where(a => a.GUID == entity.POrgLocationGUID)
                .Select(a => a.OrgLocationTypeGUIO).FirstOrDefault();
        }

        OrgLocationService.Table.Add(entity);

        await OrgLocationService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}