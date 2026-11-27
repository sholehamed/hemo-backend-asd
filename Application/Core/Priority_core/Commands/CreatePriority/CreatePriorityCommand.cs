
namespace Application.Core.Priority_core.Commands.CreatePriority;

public record CreatePriorityCommand : IRequest<Guid>, IMapFrom<Priority>
{
     
   [PropConfig("عنوان")]
   public string Name { get; init; }
    
   [PropConfig("کد")]
   public long Gkey { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreatePriorityCommand, Priority>();
    }
}

public class CreatePriorityCommandHandler : IRequestHandler<CreatePriorityCommand, Guid>
{
    private PriorityService PriorityService;
    public CreatePriorityCommandHandler(PriorityService  PriorityService)
    {
        this.PriorityService  = PriorityService;
    }

    public async Task<Guid> Handle(CreatePriorityCommand request, CancellationToken cancellationToken)
    {
        Priority entity = PriorityService.mapper.Map<Priority>(request);

        PriorityService.Table.Add(entity);

        await PriorityService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


