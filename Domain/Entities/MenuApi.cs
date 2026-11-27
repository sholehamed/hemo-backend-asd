using Domain.Attributes;

namespace Domain.Entities;

[ClassName("سرویس های منو")]
public class MenuApi : BaseEntity
{
    [PropConfig("منو")] public Guid MenuGUID { get; set; }

    [PropConfig("سرویس")] public Guid ApiGUID { get; set; }


    public virtual Api Api { get; set; } = null!;
    public virtual Menu Menu { get; set; } = null!;
}