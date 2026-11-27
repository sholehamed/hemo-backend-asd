
namespace Application.Core.TicketStatus_core.Queries;

public record TicketStatusFindByIdQuery(Guid GUID) : IRequest<TicketStatusFindByIdDTO>;

public record TicketStatusFindByIdDTO : IMapFrom<TicketStatus>
{
    
    [PropConfig("عنوان")]
    public string Name { get; init; }
    
    [PropConfig("کد")]
    public long Gkey { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<TicketStatus, TicketStatusFindByIdDTO>();
    }
}

public class
    TicketStatusFindByIdQueryHandler : IRequestHandler<TicketStatusFindByIdQuery, TicketStatusFindByIdDTO>
{
    private TicketStatusService TicketStatusService;

    public TicketStatusFindByIdQueryHandler(TicketStatusService TicketStatusService)
    {
        this.TicketStatusService = TicketStatusService;
    }

    public async Task<TicketStatusFindByIdDTO> Handle(TicketStatusFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        TicketStatusFindByIdDTO? Data = TicketStatusService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<TicketStatusFindByIdDTO>(TicketStatusService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

