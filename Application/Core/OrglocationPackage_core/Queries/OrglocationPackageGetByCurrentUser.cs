
using Application.Common.Models;
using Application.Common.Utility.Security;

namespace Application.Core.OrglocationPackage_core.Queries;

public record OrglocationPackageGetByCurrentUserQuery : PageFilter<OrglocationPackage, OrglocationPackageGetByCurrentUserDTO>
{

}

public record OrglocationPackageGetByCurrentUserDTO : IMapFrom<OrglocationPackage>
{

    [PropConfig("شرکت")] public string OrgLocationName { get; init; }

    [PropConfig("پکیج")] public string PackageName { get; init; }

    [PropConfig("اعتبار")] public DateTime ExpiryDate { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }
    [PropConfig("پکیج پنهان باشد")] public bool Hidden { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrglocationPackage, OrglocationPackageGetByCurrentUserDTO>()
            .ForMember(a => a.OrgLocationName, o => o.MapFrom(src => src.OrgLocation.Name))
            .ForMember(a => a.PackageName, o => o.MapFrom(src => src.Package.Name))
            ;
    }
}

public class
    OrglocationPackageGetByCurrentUserQueryHandler : IRequestHandler<OrglocationPackageGetByCurrentUserQuery,
    PaginatedList<OrglocationPackageGetByCurrentUserDTO>>
{
    private OrglocationPackageService OrglocationPackageService;
    private AuthService AuthService;

    public OrglocationPackageGetByCurrentUserQueryHandler(OrglocationPackageService orglocationPackageService, AuthService authService)
    {
        OrglocationPackageService = orglocationPackageService;
        AuthService = authService;
    }
    public async Task<PaginatedList<OrglocationPackageGetByCurrentUserDTO>> Handle(OrglocationPackageGetByCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        Guid UserGUID = AuthService.GetUserGUID();
        
        IQueryable<OrglocationPackage> queryable = OrglocationPackageService.Table.Where(a=>a.CreatedBy==UserGUID);
        return await request.ToPaging( queryable,OrglocationPackageService.mapper);
    }
}

