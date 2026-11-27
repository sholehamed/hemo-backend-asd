namespace Application.Core.Api_core.Commands.CreateApi;

public record CreateApiCommand : IRequest<Guid>, IMapFrom<Api>
{
    [PropConfig("مسیر درخواست ارسالی")] public string DownstreamPathTemplate { get; init; }

    [PropConfig("مسیر درخواست دریافتی")] public string UpstreamPathTemplate { get; init; }

    [PropConfig("سرویس از نوع عمومی می باشد")]
    public bool IsPublic { get; init; }

    [PropConfig("اولویت")] public int Priority { get; init; }

    [PropConfig("زیر سیستم")] public Guid SubSystemGUID { get; init; }

    [PropConfig("متد درخواست")] public string APITypeName { get; init; }

    [PropConfig("فعال")] public bool Active { get; init; }

    [PropConfig("عنوان")] public string Title { get; set; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateApiCommand, Api>();
    }
}

public class CreateApiCommandHandler : IRequestHandler<CreateApiCommand, Guid>
{
    private readonly ApiService ApiService;

    public CreateApiCommandHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task<Guid> Handle(CreateApiCommand request, CancellationToken cancellationToken)
    {
        var entity = ApiService.mapper.Map<Api>(request);

        ApiService.Table.Add(entity);

        await ApiService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}