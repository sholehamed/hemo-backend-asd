
namespace Application.Core.Discount_core.Queries;

public record DiscountFindByIdQuery(Guid GUID) : IRequest<DiscountFindByIdDTO>;

public record DiscountFindByIdDTO : IMapFrom<Discount>
{
    
    [PropConfig("کد")]
    public string Code { get; init; }
    
    [PropConfig("کد تخفیف از نوع درصدی می باشد")]
    public bool Percentage { get; init; }
    
    [PropConfig("مقدار")]
    public int Value { get; init; }
    
    [PropConfig("حداکثر مبلغ تخفیف")]
    public int? MaxDiscountAmount { get; init; }
    
    [PropConfig("حداقل مبلغ فاکتور")]
    public int MinimumOrderAmount { get; init; }
    
    [PropConfig("از تاریخ")]
    public DateTime StartDate { get; init; }
    
    [PropConfig("تا تاریخ")]
    public DateTime EndDate { get; init; }
    
    [PropConfig("کد تخفیف فعال می باشد")]
    public bool IsActive { get; init; }
    
    [PropConfig("تعداد کد تخفیف")]
    public int UsageLimit { get; init; }
    
    [PropConfig("تعداد استفاده شده")]
    public int UsedCount { get; init; }
    
    [PropConfig("حداکثر استفاده برای هر کاربر")]
    public int UsageLimitPerUser { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<Discount, DiscountFindByIdDTO>();
    }
}

public class
    DiscountFindByIdQueryHandler : IRequestHandler<DiscountFindByIdQuery, DiscountFindByIdDTO>
{
    private DiscountService DiscountService;

    public DiscountFindByIdQueryHandler(DiscountService DiscountService)
    {
        this.DiscountService = DiscountService;
    }

    public async Task<DiscountFindByIdDTO> Handle(DiscountFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        DiscountFindByIdDTO? Data = DiscountService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<DiscountFindByIdDTO>(DiscountService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

