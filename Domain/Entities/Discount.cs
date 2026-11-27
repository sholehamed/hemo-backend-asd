using Domain.Attributes;

namespace Domain.Entities;

[ClassName("کد تخفیف")]
public class Discount:AuditedEntity
{
    [PropConfig("کد",IsUnique = true)]
    public string Code { get; set; }

    [PropConfig("کد تخفیف از نوع درصدی می باشد")]
    public bool Percentage { get; set; }

    [PropConfig("مقدار")]
    public int Value { get; set; }

    [PropConfig("حداکثر مبلغ تخفیف")]
    public int? MaxDiscountAmount { get; set; }

    [PropConfig("حداقل مبلغ فاکتور")]
    public int MinimumOrderAmount { get; set; }

    [PropConfig("از تاریخ")]
    public DateTime StartDate { get; set; }

    [PropConfig("تا تاریخ")]
    public DateTime EndDate { get; set; }

    [PropConfig("کد تخفیف فعال می باشد")]
    public bool IsActive { get; set; }

    [PropConfig("تعداد کد تخفیف")]
    public int UsageLimit { get; set; }

    [PropConfig("حداکثر استفاده برای هر کاربر")]
    public int UsageLimitPerUser { get; set; }
    
    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

}