
using Application.Common.Models;

namespace Application.Core.TicketStatus_core.Queries;

public record TicketStatusWithPaginationQuery : PageFilter<TicketStatus, TicketStatusWithPaginationDTO>
{

}

public record TicketStatusWithPaginationDTO : IMapFrom<TicketStatus>
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("کد")]
    public long Gkey { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<TicketStatus, TicketStatusWithPaginationDTO>();
    }
}

public class
    TicketStatusWithPaginationQueryHandler : IRequestHandler<TicketStatusWithPaginationQuery,
    PaginatedList<TicketStatusWithPaginationDTO>>
{
    private TicketStatusService TicketStatusService;
    public TicketStatusWithPaginationQueryHandler(TicketStatusService TicketStatusService)
    {
        this.TicketStatusService = TicketStatusService;
    }

    public async Task<PaginatedList<TicketStatusWithPaginationDTO>> Handle(TicketStatusWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<TicketStatus> queryable = TicketStatusService.Table;
        return await request.ToPaging( queryable,TicketStatusService.mapper);
    }
}

