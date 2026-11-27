
namespace Application.Core.Api_core.Commands.ApiUpdateIsActive;

public record ApiUpdateIsActiveCommand : IRequest, IMapFrom<Api>
{

 
   [PropConfig("فعال")]
   public bool Active { get; init; }
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ApiUpdateIsActiveCommand, Api>();
    }
}

public class ApiUpdateIsActiveCommandHandler : IRequestHandler<ApiUpdateIsActiveCommand>
{
    private ApiService ApiService;

    public ApiUpdateIsActiveCommandHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task Handle(ApiUpdateIsActiveCommand request, CancellationToken cancellationToken)
    {
        Api? OldEntity = await ApiService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        ApiService.mapper.Map(request, OldEntity);
        
        await ApiService.SaveChangesAsync(cancellationToken);

        ApiService.UpateApiListInBackground();
    }
}

