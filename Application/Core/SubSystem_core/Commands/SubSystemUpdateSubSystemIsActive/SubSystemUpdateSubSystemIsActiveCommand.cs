
namespace Application.Core.SubSystem_core.Commands.SubSystemUpdateSubSystemIsActive;

public record SubSystemUpdateSubSystemIsActiveCommand : IRequest, IMapFrom<SubSystem>
{
    
 
   [PropConfig("فغال")]
   public bool Active { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SubSystemUpdateSubSystemIsActiveCommand, SubSystem>();
    }
}

public class SubSystemUpdateSubSystemIsActiveCommandHandler : IRequestHandler<SubSystemUpdateSubSystemIsActiveCommand>
{
    private SubSystemService SubSystemService;
    private ApiService ApiService;

    public SubSystemUpdateSubSystemIsActiveCommandHandler(SubSystemService subSystemService, ApiService apiService)
    {
        SubSystemService = subSystemService;
        ApiService = apiService;
    }

    public async Task Handle(SubSystemUpdateSubSystemIsActiveCommand request, CancellationToken cancellationToken)
    {
        SubSystem? OldEntity = await SubSystemService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        SubSystemService.mapper.Map(request, OldEntity);
        
        await SubSystemService.SaveChangesAsync(cancellationToken);

        ApiService.UpateApiListInBackground();

    }
}

