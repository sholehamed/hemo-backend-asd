

namespace Application.Core.OrglocationPackage_core.Queries;

public record OrglocationPackageFindAllQuery : IRequest<List<OrglocationPackageFindAllDTO>>
{

}

public record OrglocationPackageFindAllDTO : IMapFrom<OrglocationPackage>
{

    [PropConfig("شرکت")]
    public Guid OrgLocationGUID { get; init; }

    [PropConfig("پکیج")]
    public Guid PackageGUID { get; init; }

    [PropConfig("اعتبار")]
    public DateTime ExpiryDate { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrglocationPackage, OrglocationPackageFindAllDTO>();
    }
}

public class
    OrglocationPackageFindAllQueryHandler : IRequestHandler<OrglocationPackageFindAllQuery,
    List<OrglocationPackageFindAllDTO>>
{
    private OrglocationPackageService OrglocationPackageService;
    public OrglocationPackageFindAllQueryHandler(OrglocationPackageService OrglocationPackageService)
    {
        this.OrglocationPackageService = OrglocationPackageService;
    }

    public async Task<List<OrglocationPackageFindAllDTO>> Handle(OrglocationPackageFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await OrglocationPackageService.Table.AsNoTracking()
            .ProjectTo<OrglocationPackageFindAllDTO>(OrglocationPackageService.mapper.ConfigurationProvider).ToListAsync();
    }
}



