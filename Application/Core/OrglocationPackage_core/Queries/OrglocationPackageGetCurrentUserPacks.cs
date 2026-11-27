using Application.Common.Models;

namespace Application.Core.OrglocationPackage_core.Queries;

public record OrglocationPackageGetCurrentUserPacksQuery : IRequest<List<OrglocationPackageGetCurrentUserPacksDTO>>
{
}

public record OrglocationPackageGetCurrentUserPacksDTO : IMapFrom<OrglocationPackage>
{
    [PropConfig("پکیج")] public string PackageName { get; init; }
    [PropConfig("کلید")] public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrglocationPackage, OrglocationPackageGetCurrentUserPacksDTO>();
    }
}

public class
    OrglocationPackageGetCurrentUserPacksQueryHandler : IRequestHandler<OrglocationPackageGetCurrentUserPacksQuery,
    List<OrglocationPackageGetCurrentUserPacksDTO>>
{
    private UserRoleService UserRoleService;
    private OrglocationPackageService OrglocationPackageService;

    public OrglocationPackageGetCurrentUserPacksQueryHandler(UserRoleService userRoleService,
        OrglocationPackageService orglocationPackageService)
    {
        UserRoleService = userRoleService;
        OrglocationPackageService = orglocationPackageService;
    }

    public async Task<List<OrglocationPackageGetCurrentUserPacksDTO>> Handle(
        OrglocationPackageGetCurrentUserPacksQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        DateTime NowDate = DateTime.Now;
        return await OrglocationPackageService.Table.Where(a =>
               
                a.ExpiryDate > NowDate && a.OrgLocationGUID == userRoleInfo.OrgLocationGUID).AsNoTracking()
            .ProjectTo<OrglocationPackageGetCurrentUserPacksDTO>(OrglocationPackageService.mapper.ConfigurationProvider)
            .ToListAsync();
    }
}