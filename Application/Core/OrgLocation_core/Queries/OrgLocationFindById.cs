
namespace Application.Core.OrgLocation_core.Queries;

public record OrgLocationFindByIdQuery(Guid GUID) : IRequest<OrgLocationFindByIdDTO>;

public record OrgLocationFindByIdDTO : IMapFrom<OrgLocation>
{
    
    [PropConfig("عنوان")]
    public string Name { get; init; }
    
    [PropConfig("آدرس")]
    public string? Address { get; init; }
    
    [PropConfig("تلفن")]
    public string? Tell { get; init; }
    
    [PropConfig("فکس")]
    public string? Fax { get; init; }
    
    [PropConfig("ایمیل")]
    public string? Email { get; init; }
    
    [PropConfig("آدرس وبسایت")]
    public string? Url { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrgLocation, OrgLocationFindByIdDTO>();
    }
}

public class
    OrgLocationFindByIdQueryHandler : IRequestHandler<OrgLocationFindByIdQuery, OrgLocationFindByIdDTO>
{
    private OrgLocationService OrgLocationService;

    public OrgLocationFindByIdQueryHandler(OrgLocationService OrgLocationService)
    {
        this.OrgLocationService = OrgLocationService;
    }

    public async Task<OrgLocationFindByIdDTO> Handle(OrgLocationFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        await OrgLocationService.OrgLocationIsValid(request.GUID);
        
        OrgLocationFindByIdDTO? Data = OrgLocationService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<OrgLocationFindByIdDTO>(OrgLocationService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

