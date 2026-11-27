
using Application.Common.Models;

namespace Application.Core.sso_core.Queries;

public record ssoWithPaginationQuery : PageFilter<sso, ssoWithPaginationDTO>
{

}

public record ssoWithPaginationDTO : IMapFrom<sso>
{

    [PropConfig("کاربر")]
    public Guid UserGUID { get; init; }

    [PropConfig("پکیج")]
    public Guid PackageGUID { get; init; }

    [PropConfig("آدرس IP")]
    public string IPAddress { get; init; }

    [PropConfig("مرورگر")]
    public string UserAgent { get; init; }

    [PropConfig("تاریخ انقضا")]
    public DateTime ExpireDate { get; init; }

    [PropConfig("کلید")]
    public string Key { get; init; }

    [PropConfig("اعتبارسنجی")]
    public string Sign { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<sso, ssoWithPaginationDTO>();
    }
}

public class
    ssoWithPaginationQueryHandler : IRequestHandler<ssoWithPaginationQuery,
    PaginatedList<ssoWithPaginationDTO>>
{
    private ssoService ssoService;
    public ssoWithPaginationQueryHandler(ssoService ssoService)
    {
        this.ssoService = ssoService;
    }

    public async Task<PaginatedList<ssoWithPaginationDTO>> Handle(ssoWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<sso> queryable = ssoService.Table;
        return await request.ToPaging( queryable,ssoService.mapper);
    }
}

