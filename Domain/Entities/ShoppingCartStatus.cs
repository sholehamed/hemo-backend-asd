using Domain.Attributes;

namespace Domain.Entities;

[ClassName("وضعیت سبد خرید")]
public class ShoppingCartStatus : BaseEntity
{
    [PropConfig("عنوان")] public string Name { get; set; }
    [PropConfig("کد", IsUnique: true)] public int Gkey { get; set; }
    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
}