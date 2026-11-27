using Domain.Attributes;

namespace Domain.Entities;

[ClassName("سبد خرید")]
public class ShoppingCart : BaseEntity
{
    [PropConfig("وضعیت سبد خرید")] public Guid ShoppingCartStatusGUID { get; set; }
    [PropConfig("کاربر ثبت کننده")] public Guid UserGUID { get; set; }

    [PropConfig("کد برداشت از کیف پول")] public Guid? TransactionGUID { get; set; }

    [PropConfig("کد تخفیف")] public Guid? DiscountGUID { get; set; }
    [PropConfig("مبلغ کل فاکتور")] public long? OrderAmount { get; set; }
    [PropConfig("مبلغ تخفیف")] public long? DiscountAmount { get; set; }
    [PropConfig("مبلغ قابل پرداخت")] public long? FinalAmount { get; set; }
    [PropConfig("مالیات بر ارزش افزوده")] public long? Tax { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual ShoppingCartStatus ShoppingCartStatus { get; set; } = null!;
    public virtual Discount? Discount { get; set; } 
    public virtual ICollection<ShoppingCartProduct> ShoppingCartProducts { get; set; } =
        new List<ShoppingCartProduct>();
}