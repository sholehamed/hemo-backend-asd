namespace Application.Core.Api_core.Queries;

public record ApiFindByIdQuery(Guid GUID) : IRequest<ApiFindByIdDTO>;

public record ApiFindByIdDTO : IMapFrom<Api>
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
        profile.CreateMap<Api, ApiFindByIdDTO>();
    }
}

public class
    ApiFindByIdQueryHandler : IRequestHandler<ApiFindByIdQuery, ApiFindByIdDTO>
{
    private readonly ApiService ApiService;

    public ApiFindByIdQueryHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task<ApiFindByIdDTO> Handle(ApiFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        var Data = ApiService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<ApiFindByIdDTO>(ApiService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}