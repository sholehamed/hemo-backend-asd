
namespace Application.Core.TicketStatus_core.Commands.CreateTicketStatus;

public record CreateTicketStatusCommand : IRequest<Guid>, IMapFrom<TicketStatus>
{
     
   [PropConfig("عنوان")]
   public string Name { get; init; }
    
   [PropConfig("کد")]
   public long Gkey { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateTicketStatusCommand, TicketStatus>();
    }
}

public class CreateTicketStatusCommandHandler : IRequestHandler<CreateTicketStatusCommand, Guid>
{
    private TicketStatusService TicketStatusService;
    public CreateTicketStatusCommandHandler(TicketStatusService  TicketStatusService)
    {
        this.TicketStatusService  = TicketStatusService;
    }

    public async Task<Guid> Handle(CreateTicketStatusCommand request, CancellationToken cancellationToken)
    {
        TicketStatus entity = TicketStatusService.mapper.Map<TicketStatus>(request);

        TicketStatusService.Table.Add(entity);

        await TicketStatusService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


