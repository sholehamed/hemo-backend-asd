using Application.Common.Exceptions;
using Application.Common.Utility.Security;

namespace Application.Repository.Services;

public class DiscountService : BaseRepository<Discount>
{
    private AuthService AuthService;

    public DiscountService(IApplicationDbContext context, IMapper mapper, AuthService authService) : base(context,
        mapper)
    {
        AuthService = authService;
    }

    public async Task<(long FinalPrice, long Discount, Guid DiscountGUID)> ApplyDiscount(Guid ShoppingCartGUID,
        Guid DiscountGUID, long orderAmount)
    {
        Discount? discount = db.Discount.FirstOrDefault(a => a.GUID == DiscountGUID);
        return await ApplyDiscount(ShoppingCartGUID, discount.Code, orderAmount);
    }

    public async Task<(long FinalPrice, long Discount, Guid DiscountGUID)> ApplyDiscount(Guid ShoppingCartGUID,
        string DiscountCode, long orderAmount)
    {
        Discount? discount = db.Discount.FirstOrDefault(a => a.Code == DiscountCode);
        if (discount == null)
            throw new ServiceExeption("کد تخفیف معتبر نمی باشد.");

        if (!discount.IsActive)
            throw new ServiceExeption("کد تخفیف غیرفعال است");

        if (DateTime.Now < discount.StartDate)
            throw new ServiceExeption("کد تخفیف فعال نشده است.");

        if (DateTime.Now > discount.EndDate)
            throw new ServiceExeption("کد تخفیف منقضی شده است.");

        int UsedCount = db.ShoppingCart.IgnoreQueryFilters().Count(a => a.GUID != ShoppingCartGUID && a.DiscountGUID == discount.GUID);
        if (discount.UsageLimit <= UsedCount)
            throw new ServiceExeption("ظرفیت استفاده از کد تخفیف تکمیل شده است.");

        if (discount.UsageLimitPerUser > 0)
        {
            Guid UserGUID = AuthService.GetUserGUID();
            int UserUsedCount = db.ShoppingCart.IgnoreQueryFilters().Count(a =>
                a.UserGUID == UserGUID && a.GUID != ShoppingCartGUID && a.DiscountGUID == discount.GUID);

            if (discount.UsageLimitPerUser <= UserUsedCount)
                throw new ServiceExeption("ظرفیت استفاده از کد تخفیف تکمیل شده است.");
        }

        if (
            orderAmount < discount.MinimumOrderAmount)
            throw new ServiceExeption(
                $"حداقل مبلغ سفارش جهت استفاده از این کد تخفیف {discount.MinimumOrderAmount} ریال می باشد.");

        long discountAmount = 0;

        if (discount.Percentage)
        {
            discountAmount = orderAmount * discount.Value / 100;

            if (discount.MaxDiscountAmount.HasValue &&
                discountAmount > discount.MaxDiscountAmount)
            {
                discountAmount = discount.MaxDiscountAmount.Value;
            }

           
        }
        else if (!discount.Percentage)
        {
            discountAmount = discount.Value;
        }

        if (discountAmount > orderAmount)
        {
            discountAmount = orderAmount;
        }
        
        var finalPrice = orderAmount - discountAmount;

        if (finalPrice < 0)
            finalPrice = 0;

        return (finalPrice, discountAmount, discount.GUID);
    }
}