namespace Application.Core.Api_core.Commands.ApiUpdateIntranet;

public record ApiUpdateIntranetCommand : IRequest, IMapFrom<Api>
{
    [PropConfig("قابل نمایش در اینترانت")] public bool Intranet { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ApiUpdateIntranetCommand, Api>();
    }
}

public class ApiUpdateIntranetCommandHandler : IRequestHandler<ApiUpdateIntranetCommand>
{
    private ApiService ApiService;

    public ApiUpdateIntranetCommandHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task Handle(ApiUpdateIntranetCommand request, CancellationToken cancellationToken)
    {
        Api? OldEntity = await ApiService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        ApiService.mapper.Map(request, OldEntity);

        await ApiService.SaveChangesAsync(cancellationToken);
    }
}