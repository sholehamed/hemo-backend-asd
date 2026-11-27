using Domain.Attributes;

namespace Domain.Entities;

[ClassName("محصولات سبد خرید")]
public class ShoppingCartProduct : BaseEntity
{
    [PropConfig("پکیج")] public Guid PackageGUID { get; set; }
    [PropConfig("سبد خرید")] public Guid ShoppingCartGUID { get; set; }
    [PropConfig("شرکت")] public Guid OrgLocationGUID { get; set; }
    [PropConfig("مبلغ")] public long? Price { get; set; }
  
    [PropConfig("تمدید")] public bool Renew { get; set; }

    public virtual OrgLocation OrgLocation { get; set; } = null!;
    public virtual Package Package { get; set; } = null!;
    public virtual ShoppingCart ShoppingCart { get; set; } = null!;
}