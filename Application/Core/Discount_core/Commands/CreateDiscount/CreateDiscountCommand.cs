
using Application.Common.Exceptions;

namespace Application.Core.Discount_core.Commands.CreateDiscount;

public record CreateDiscountCommand : IRequest<Guid>, IMapFrom<Discount>
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

    
   [PropConfig("حداکثر استفاده برای هر کاربر")]
   public int UsageLimitPerUser { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateDiscountCommand, Discount>();
    }
}

public class CreateDiscountCommandHandler : IRequestHandler<CreateDiscountCommand, Guid>
{
    private DiscountService DiscountService;
    public CreateDiscountCommandHandler(DiscountService  DiscountService)
    {
        this.DiscountService  = DiscountService;
    }

    public async Task<Guid> Handle(CreateDiscountCommand request, CancellationToken cancellationToken)
    {
        bool Exist=DiscountService.db.Discount.Any(a=>a.Code==request.Code);
        if (Exist)
        {
            throw new ServiceExeption("کد تخفیف وارد شده قبلا ثبت شده است.");
        }
        
        Discount entity = DiscountService.mapper.Map<Discount>(request);

        DiscountService.Table.Add(entity);

        await DiscountService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


