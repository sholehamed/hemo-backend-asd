
namespace Application.Core.Priority_core.Commands.UpdatePriority;

public record UpdatePriorityCommand : IRequest, IMapFrom<Priority>
{
 
   [PropConfig("عنوان")]
   public string Name { get; init; }
 
   [PropConfig("کد")]
   public long Gkey { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdatePriorityCommand, Priority>();
    }
}

public class UpdatePriorityCommandHandler : IRequestHandler<UpdatePriorityCommand>
{
    private PriorityService PriorityService;

    public UpdatePriorityCommandHandler(PriorityService PriorityService)
    {
        this.PriorityService = PriorityService;
    }

    public async Task Handle(UpdatePriorityCommand request, CancellationToken cancellationToken)
    {
        Priority? OldEntity = await PriorityService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        PriorityService.mapper.Map(request, OldEntity);
        
        await PriorityService.SaveChangesAsync(cancellationToken);
    }
}

