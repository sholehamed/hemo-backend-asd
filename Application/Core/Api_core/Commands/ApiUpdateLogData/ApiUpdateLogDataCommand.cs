
namespace Application.Core.Api_core.Commands.ApiUpdateLogData;

public record ApiUpdateLogDataCommand : IRequest, IMapFrom<Api>
{
 
  [PropConfig("لاگ فعال باشد")]
   public bool LogData { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }
 

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ApiUpdateLogDataCommand, Api>();
    }
}

public class ApiUpdateLogDataCommandHandler : IRequestHandler<ApiUpdateLogDataCommand>
{
    private ApiService ApiService;

    public ApiUpdateLogDataCommandHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task Handle(ApiUpdateLogDataCommand request, CancellationToken cancellationToken)
    {
        Api? OldEntity = await ApiService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        ApiService.mapper.Map(request, OldEntity);
        
        await ApiService.SaveChangesAsync(cancellationToken);
    }
}

