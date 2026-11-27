
using Application.Common.Models;

namespace Application.Core.Discount_core.Queries;

public record DiscountWithPaginationQuery : PageFilter<Discount, DiscountWithPaginationDTO>
{

}

public record DiscountWithPaginationDTO : IMapFrom<Discount>
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
        profile.CreateMap<Discount, DiscountWithPaginationDTO>();
    }
}

public class
    DiscountWithPaginationQueryHandler : IRequestHandler<DiscountWithPaginationQuery,
    PaginatedList<DiscountWithPaginationDTO>>
{
    private DiscountService DiscountService;
    public DiscountWithPaginationQueryHandler(DiscountService DiscountService)
    {
        this.DiscountService = DiscountService;
    }

    public async Task<PaginatedList<DiscountWithPaginationDTO>> Handle(DiscountWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<Discount> queryable = DiscountService.Table;
        return await request.ToPaging( queryable,DiscountService.mapper);
    }
}

