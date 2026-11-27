namespace Application.Core.Api_core.Commands.ApiUpdateInternet;

public record ApiUpdateInternetCommand : IRequest, IMapFrom<Api>
{
    [PropConfig("قابل نمایش در اینترنت")] public bool Internet { get; init; }
    [PropConfig("کلید")] public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ApiUpdateInternetCommand, Api>();
    }
}

public class ApiUpdateInternetCommandHandler : IRequestHandler<ApiUpdateInternetCommand>
{
    private ApiService ApiService;

    public ApiUpdateInternetCommandHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task Handle(ApiUpdateInternetCommand request, CancellationToken cancellationToken)
    {
        Api? OldEntity = await ApiService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        ApiService.mapper.Map(request, OldEntity);

        await ApiService.SaveChangesAsync(cancellationToken);
    }
}