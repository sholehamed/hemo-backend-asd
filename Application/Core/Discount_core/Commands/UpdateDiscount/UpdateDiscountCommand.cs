
using Application.Common.Exceptions;

namespace Application.Core.Discount_core.Commands.UpdateDiscount;

public record UpdateDiscountCommand : IRequest, IMapFrom<Discount>
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
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateDiscountCommand, Discount>();
    }
}

public class UpdateDiscountCommandHandler : IRequestHandler<UpdateDiscountCommand>
{
    private DiscountService DiscountService;

    public UpdateDiscountCommandHandler(DiscountService DiscountService)
    {
        this.DiscountService = DiscountService;
    }

    public async Task Handle(UpdateDiscountCommand request, CancellationToken cancellationToken)
    {
        bool Exist=DiscountService.db.Discount.Any(a=>a.GUID!=request.GUID&&a.Code==request.Code);
        if (Exist)
        {
            throw new ServiceExeption("کد تخفیف وارد شده قبلا ثبت شده است.");
        }
        
        Discount? OldEntity = await DiscountService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        DiscountService.mapper.Map(request, OldEntity);
        
        await DiscountService.SaveChangesAsync(cancellationToken);
    }
}

