
namespace Application.Core.DemoRequest_core.Queries;

public record DemoRequestFindByIdQuery(Guid GUID) : IRequest<DemoRequestFindByIdDTO>;

public record DemoRequestFindByIdDTO : IMapFrom<DemoRequest>
{
    
    [PropConfig("کاربر ثبت کننده")]
    public Guid UserGUID { get; init; }
    
    [PropConfig("پکیج")]
    public Guid PackageGUID { get; init; }
    
    [PropConfig("شرکت")]
    public Guid OrgLocationGUID { get; init; }
    
    [PropConfig("قبول شده")]
    public bool? Accepted { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }
    
    [PropConfig("کد کانورت دیتا")]
    public string? ConvertID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<DemoRequest, DemoRequestFindByIdDTO>();
    }
}

public class
    DemoRequestFindByIdQueryHandler : IRequestHandler<DemoRequestFindByIdQuery, DemoRequestFindByIdDTO>
{
    private DemoRequestService DemoRequestService;

    public DemoRequestFindByIdQueryHandler(DemoRequestService DemoRequestService)
    {
        this.DemoRequestService = DemoRequestService;
    }

    public async Task<DemoRequestFindByIdDTO> Handle(DemoRequestFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        DemoRequestFindByIdDTO? Data = DemoRequestService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<DemoRequestFindByIdDTO>(DemoRequestService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

