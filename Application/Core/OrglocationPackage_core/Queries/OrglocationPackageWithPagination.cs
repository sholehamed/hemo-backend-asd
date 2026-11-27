using Application.Common.Models;

namespace Application.Core.OrglocationPackage_core.Queries;

public record
    OrglocationPackageWithPaginationQuery : PageFilter<OrglocationPackage, OrglocationPackageWithPaginationDTO>
{
    [PropConfig("شرکت")] public Guid? OrgLocationGUID { get; set; }
}

public record OrglocationPackageWithPaginationDTO : IMapFrom<OrglocationPackage>
{
    [PropConfig("شرکت")] public string OrgLocationName { get; init; }

    [PropConfig("پکیج")] public string PackageName { get; init; }

    [PropConfig("اعتبار")] public DateTime ExpiryDate { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrglocationPackage, OrglocationPackageWithPaginationDTO>()
            .ForMember(a => a.OrgLocationName, o => o.MapFrom(src => src.OrgLocation.Name))
            .ForMember(a => a.PackageName, o => o.MapFrom(src => src.Package.Name))
            ;
    }
}

public class
    OrglocationPackageWithPaginationQueryHandler : IRequestHandler<OrglocationPackageWithPaginationQuery,
    PaginatedList<OrglocationPackageWithPaginationDTO>>
{
    private OrglocationPackageService OrglocationPackageService;

    public OrglocationPackageWithPaginationQueryHandler(OrglocationPackageService OrglocationPackageService)
    {
        this.OrglocationPackageService = OrglocationPackageService;
    }

    public async Task<PaginatedList<OrglocationPackageWithPaginationDTO>> Handle(
        OrglocationPackageWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<OrglocationPackage> queryable = OrglocationPackageService.Table;
        if(request.OrgLocationGUID is not null)
        {
            queryable = queryable.Where(s => s.OrgLocationGUID == request.OrgLocationGUID);
        }
        return await request.ToPaging(queryable, OrglocationPackageService.mapper);
    }
}