
using Application.Common.Models;

namespace Application.Core.Priority_core.Queries;

public record PriorityWithPaginationQuery : PageFilter<Priority, PriorityWithPaginationDTO>
{

}

public record PriorityWithPaginationDTO : IMapFrom<Priority>
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("کد")]
    public long Gkey { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<Priority, PriorityWithPaginationDTO>();
    }
}

public class
    PriorityWithPaginationQueryHandler : IRequestHandler<PriorityWithPaginationQuery,
    PaginatedList<PriorityWithPaginationDTO>>
{
    private PriorityService PriorityService;
    public PriorityWithPaginationQueryHandler(PriorityService PriorityService)
    {
        this.PriorityService = PriorityService;
    }

    public async Task<PaginatedList<PriorityWithPaginationDTO>> Handle(PriorityWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Priority> queryable = PriorityService.Table;
        return await request.ToPaging( queryable,PriorityService.mapper);
    }
}

