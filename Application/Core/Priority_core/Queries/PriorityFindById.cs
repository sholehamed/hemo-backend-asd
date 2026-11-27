
namespace Application.Core.Priority_core.Queries;

public record PriorityFindByIdQuery(Guid GUID) : IRequest<PriorityFindByIdDTO>;

public record PriorityFindByIdDTO : IMapFrom<Priority>
{
    
    [PropConfig("عنوان")]
    public string Name { get; init; }
    
    [PropConfig("کد")]
    public long Gkey { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<Priority, PriorityFindByIdDTO>();
    }
}

public class
    PriorityFindByIdQueryHandler : IRequestHandler<PriorityFindByIdQuery, PriorityFindByIdDTO>
{
    private PriorityService PriorityService;

    public PriorityFindByIdQueryHandler(PriorityService PriorityService)
    {
        this.PriorityService = PriorityService;
    }

    public async Task<PriorityFindByIdDTO> Handle(PriorityFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        PriorityFindByIdDTO? Data = PriorityService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<PriorityFindByIdDTO>(PriorityService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

