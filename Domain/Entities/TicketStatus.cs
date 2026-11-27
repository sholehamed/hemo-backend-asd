using Domain.Attributes;

namespace Domain.Entities;

[ClassName("وضعیت تیکت")]
public class TicketStatus:BaseEntity
{
    [PropConfig("عنوان")]
    public string Name { get; set; }
    [PropConfig("کد",IsUnique = true)]
    public long Gkey { get; set; }
    
    [PropConfig("Style")]
    public string Style { get; set; }
    
    public virtual ICollection<Ticket> Tickets { get; set; }

}