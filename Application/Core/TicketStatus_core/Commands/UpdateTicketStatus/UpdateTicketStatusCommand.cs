
namespace Application.Core.TicketStatus_core.Commands.UpdateTicketStatus;

public record UpdateTicketStatusCommand : IRequest, IMapFrom<TicketStatus>
{
 
   [PropConfig("عنوان")]
   public string Name { get; init; }
 
   [PropConfig("کد")]
   public long Gkey { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateTicketStatusCommand, TicketStatus>();
    }
}

public class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketStatusCommand>
{
    private TicketStatusService TicketStatusService;

    public UpdateTicketStatusCommandHandler(TicketStatusService TicketStatusService)
    {
        this.TicketStatusService = TicketStatusService;
    }

    public async Task Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
    {
        TicketStatus? OldEntity = await TicketStatusService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        TicketStatusService.mapper.Map(request, OldEntity);
        
        await TicketStatusService.SaveChangesAsync(cancellationToken);
    }
}

