namespace Application.Core.Api_core.Commands.UpdateApi;

public record UpdateApiCommand : IRequest, IMapFrom<Api>
{


    [PropConfig("سرویس از نوع عمومی می باشد")]
    public bool IsPublic { get; set; }
    [PropConfig("فعال")] public bool Active { get; set; }
    [PropConfig("عنوان")] public string Title { get; set; }

    [PropConfig("قابل نمایش در اینترنت")]
    public bool Internet { get; set; }
    [PropConfig("قابل نمایش در اینترانت")]
    public bool Intranet { get; set; }
    
    [PropConfig("لاگ فعال باشد")]
    public bool LogData { get; set; }

    [PropConfig("اطلاعات حساس")]
    public string? Sensitive { get; set; }
    [PropConfig("هدر درخواست کاربر لاگ شود")]
    public bool LogRequestHeaders { get; set; }
    [PropConfig("هدر پاسخ سرویس لاگ شود")]
    public bool LogResponseHeaders { get; set; }
    [PropConfig("بدنه درخواست کاربر لاگ شود")]
    public bool LogRequestBody { get; set; }
    [PropConfig("بدنه پاسخ سرویس لاگ شود")]
    public bool LogResponseBody { get; set; }
    [PropConfig("ریت لیمیت فعال باشد")]
    public bool EnableRateLimiting { get; set; } = false;
    [PropConfig("بازه زمانی")]
    public int? Period { get; set; } = null;
    
    
    [PropConfig("تعداد درخواست")]
    public int Limit { get; set; }
    [PropConfig("کلید")]
    public Guid GUID { get; init; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateApiCommand, Api>();
    }
}

public class UpdateApiCommandHandler : IRequestHandler<UpdateApiCommand>
{
    private readonly ApiService ApiService;

    public UpdateApiCommandHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task Handle(UpdateApiCommand request, CancellationToken cancellationToken)
    {
        var OldEntity = await ApiService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);

        ApiService.mapper.Map(request, OldEntity);

        await ApiService.SaveChangesAsync(cancellationToken);
    }
}