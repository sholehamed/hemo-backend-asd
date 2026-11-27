

namespace Application.Core.sso_core.Queries;

public record ssoFindAllQuery : IRequest<List<ssoFindAllDTO>>
{

}

public record ssoFindAllDTO : IMapFrom<sso>
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
        profile.CreateMap<sso, ssoFindAllDTO>();
    }
}

public class
    ssoFindAllQueryHandler : IRequestHandler<ssoFindAllQuery,
    List<ssoFindAllDTO>>
{
    private ssoService ssoService;
    public ssoFindAllQueryHandler(ssoService ssoService)
    {
        this.ssoService = ssoService;
    }

    public async Task<List<ssoFindAllDTO>> Handle(ssoFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await ssoService.Table.AsNoTracking()
            .ProjectTo<ssoFindAllDTO>(ssoService.mapper.ConfigurationProvider).ToListAsync();
    }
}



