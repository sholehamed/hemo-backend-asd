using Domain.Attributes;

namespace Domain.Entities;

[ClassName("دپارتمان")]
public class Department:BaseEntity
{
    [PropConfig("عنوان")]
    public string Name { get; set; }
    [PropConfig("کد",IsUnique = true)]
    public long Gkey { get; set; }
    
    public virtual ICollection<Ticket> Tickets { get; set; }
}