

namespace Application.Core.TicketStatus_core.Queries;

public record TicketStatusFindAllQuery : IRequest<List<TicketStatusFindAllDTO>>
{

}

public record TicketStatusFindAllDTO : IMapFrom<TicketStatus>
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("Selected")]
    public bool Selected { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }
    [PropConfig("Style")]
    public string Style { get; set; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<TicketStatus, TicketStatusFindAllDTO>();
    }
}

public class
    TicketStatusFindAllQueryHandler : IRequestHandler<TicketStatusFindAllQuery,
    List<TicketStatusFindAllDTO>>
{
    private TicketStatusService TicketStatusService;
    public TicketStatusFindAllQueryHandler(TicketStatusService TicketStatusService)
    {
        this.TicketStatusService = TicketStatusService;
    }

    public async Task<List<TicketStatusFindAllDTO>> Handle(TicketStatusFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        var data= await TicketStatusService.Table.AsNoTracking().OrderBy(a=>a.Gkey)
            .ProjectTo<TicketStatusFindAllDTO>(TicketStatusService.mapper.ConfigurationProvider).ToListAsync();

        data.Insert(0, new TicketStatusFindAllDTO()
        {
            Selected = true,
            GUID = Guid.Empty,
            Name = "همه تیکت ها"
        });
        
        return data;
    }
}



