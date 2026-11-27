
namespace Application.Core.OrglocationPackage_core.Queries;

public record OrglocationPackageFindByIdQuery(Guid GUID) : IRequest<OrglocationPackageFindByIdDTO>;

public record OrglocationPackageFindByIdDTO : IMapFrom<OrglocationPackage>
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
        profile.CreateMap<OrglocationPackage, OrglocationPackageFindByIdDTO>();
    }
}

public class
    OrglocationPackageFindByIdQueryHandler : IRequestHandler<OrglocationPackageFindByIdQuery, OrglocationPackageFindByIdDTO>
{
    private OrglocationPackageService OrglocationPackageService;

    public OrglocationPackageFindByIdQueryHandler(OrglocationPackageService OrglocationPackageService)
    {
        this.OrglocationPackageService = OrglocationPackageService;
    }

    public async Task<OrglocationPackageFindByIdDTO> Handle(OrglocationPackageFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        OrglocationPackageFindByIdDTO? Data = OrglocationPackageService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<OrglocationPackageFindByIdDTO>(OrglocationPackageService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

