
namespace Application.Core.sso_core.Queries;

public record ssoFindByIdQuery(Guid GUID) : IRequest<ssoFindByIdDTO>;

public record ssoFindByIdDTO : IMapFrom<sso>
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
        profile.CreateMap<sso, ssoFindByIdDTO>();
    }
}

public class
    ssoFindByIdQueryHandler : IRequestHandler<ssoFindByIdQuery, ssoFindByIdDTO>
{
    private ssoService ssoService;

    public ssoFindByIdQueryHandler(ssoService ssoService)
    {
        this.ssoService = ssoService;
    }

    public async Task<ssoFindByIdDTO> Handle(ssoFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        ssoFindByIdDTO? Data = ssoService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<ssoFindByIdDTO>(ssoService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

