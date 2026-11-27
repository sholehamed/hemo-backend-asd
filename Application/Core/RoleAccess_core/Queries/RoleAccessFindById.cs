
namespace Application.Core.RoleAccess_core.Queries;

public record RoleAccessFindByIdQuery(Guid GUID) : IRequest<RoleAccessFindByIdDTO>;

public record RoleAccessFindByIdDTO : IMapFrom<RoleAccess>
{
    
    [PropConfig("منو")]
    public Guid MenuGUID { get; init; }
    
    [PropConfig("نقش")]
    public Guid RoleGUID { get; init; }
    
    [PropConfig("سرویس")]
    public Guid ApiGUID { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<RoleAccess, RoleAccessFindByIdDTO>();
    }
}

public class
    RoleAccessFindByIdQueryHandler : IRequestHandler<RoleAccessFindByIdQuery, RoleAccessFindByIdDTO>
{
    private RoleAccessService RoleAccessService;

    public RoleAccessFindByIdQueryHandler(RoleAccessService RoleAccessService)
    {
        this.RoleAccessService = RoleAccessService;
    }

    public async Task<RoleAccessFindByIdDTO> Handle(RoleAccessFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        RoleAccessFindByIdDTO? Data = RoleAccessService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<RoleAccessFindByIdDTO>(RoleAccessService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

