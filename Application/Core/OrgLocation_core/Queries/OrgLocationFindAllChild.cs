using Application.Common.Models;
using UserRole = Domain.Entities.UserRole;

namespace Application.Core.OrgLocation_core.Queries;

public record OrgLocationFindAllChildQuery : IRequest<List<OrgLocationFindAllChildDTO>>
{
}

public record OrgLocationFindAllChildDTO : IMapFrom<OrgLocation>
{
    [PropConfig("عنوان")] public string Name { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }

    [PropConfig("صاحب شرکت")] public bool Owner { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrgLocation, OrgLocationFindAllChildDTO>();
    }
}

public class
    OrgLocationFindAllChildQueryHandler : IRequestHandler<OrgLocationFindAllChildQuery,
    List<OrgLocationFindAllChildDTO>>
{
    private UserRoleService UserRoleService;
    private OrgLocationService OrgLocationService;

    public OrgLocationFindAllChildQueryHandler(UserRoleService userRoleService, OrgLocationService orgLocationService)
    {
        UserRoleService = userRoleService;
        OrgLocationService = orgLocationService;
    }

    public async Task<List<OrgLocationFindAllChildDTO>> Handle(OrgLocationFindAllChildQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();

        if (userRoleInfo.IsSuperAdmin)
        {
            List<OrgLocationFindAllChildDTO> Data = await OrgLocationService.Table.AsNoTracking()
                .Where(a => a.Gkey == 1
                ).OrderBy(location => location.Gkey)
                .ProjectTo<OrgLocationFindAllChildDTO>(OrgLocationService.mapper.ConfigurationProvider).ToListAsync();
            Data.ForEach(item => item.Owner = true);
            return Data;
        }
        else
        {
            string userRoleOrgLocationPath =
                OrgLocationService.db.UserRole.Where(a => a.UserGUID == userRoleInfo.UserGuid && a.Role.Gkey == 2)
                    .Select(a => a.OrgLocation.Path).First();

            List<OrgLocationFindAllChildDTO> Owner = await OrgLocationService.Table.AsNoTracking()
                .Where(a => a.Path.StartsWith(userRoleOrgLocationPath)
                ).OrderBy(location => location.Gkey)
                .ProjectTo<OrgLocationFindAllChildDTO>(OrgLocationService.mapper.ConfigurationProvider).ToListAsync();

            Owner.ForEach(item => item.Owner = true);

            
            List<OrgLocationFindAllChildDTO> Invite = await OrgLocationService.Table.AsNoTracking()
                .Where(a =>
                    a.UserRoles.Any(b => b.UserGUID == userRoleInfo.UserGuid&&b.ExpireDate> DateTime.Now) &&
                    a.OrglocationPackages.Any()
                ).OrderBy(location => location.Gkey)
                .ProjectTo<OrgLocationFindAllChildDTO>(OrgLocationService.mapper.ConfigurationProvider).ToListAsync();

            Owner.AddRange(Invite.Where(a => Owner.All(b => b.GUID != a.GUID)));

            return Owner;
        }
    }
}