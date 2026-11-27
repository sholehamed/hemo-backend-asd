
namespace Application.Core.Api_core.Commands.ApiUpdateIsPublic;

public record ApiUpdateIsPublicCommand : IRequest, IMapFrom<Api>
{
 

   [PropConfig("سرویس از نوع عمومی می باشد")]
   public bool IsPublic { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ApiUpdateIsPublicCommand, Api>();
    }
}

public class ApiUpdateIsPublicCommandHandler : IRequestHandler<ApiUpdateIsPublicCommand>
{
    private ApiService ApiService;

    public ApiUpdateIsPublicCommandHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task Handle(ApiUpdateIsPublicCommand request, CancellationToken cancellationToken)
    {
        Api? OldEntity = await ApiService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        ApiService.mapper.Map(request, OldEntity);
        
        await ApiService.SaveChangesAsync(cancellationToken);
        
        ApiService.UpateApiListInBackground();
    }
}

