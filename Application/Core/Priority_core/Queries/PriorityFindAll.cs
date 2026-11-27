

namespace Application.Core.Priority_core.Queries;

public record PriorityFindAllQuery : IRequest<List<PriorityFindAllDTO>>
{

}

public record PriorityFindAllDTO : IMapFrom<Priority>
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("کد")]
    public long Gkey { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<Priority, PriorityFindAllDTO>();
    }
}

public class
    PriorityFindAllQueryHandler : IRequestHandler<PriorityFindAllQuery,
    List<PriorityFindAllDTO>>
{
    private PriorityService PriorityService;
    public PriorityFindAllQueryHandler(PriorityService PriorityService)
    {
        this.PriorityService = PriorityService;
    }

    public async Task<List<PriorityFindAllDTO>> Handle(PriorityFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await PriorityService.Table.AsNoTracking()
            .ProjectTo<PriorityFindAllDTO>(PriorityService.mapper.ConfigurationProvider).ToListAsync();
    }
}



