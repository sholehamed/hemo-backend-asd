using Domain.Attributes;

namespace Domain.Entities;

[ClassName("تیکت")]
public class Ticket:AuditedEntity
{
    [PropConfig("عنوان")]
    public string Title { get; set; }
    
    [PropConfig("وضعیت")]
    public Guid TicketStatusGUID { get; set; }
    
    [PropConfig("دپارتمان")]
    public Guid DepartmentGUID { get; set; }
    
    [PropConfig("اولویت")]
    public Guid PriorityGUID { get; set; }

    [PropConfig("عنوان")]
    public long TicketNumber { get; set; }
    
    [PropConfig("کاربر ارسال کننده")]
    public Guid UserGUID { get; set; }
    
    public virtual TicketStatus TicketStatus { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
    public virtual Priority Priority { get; set; } = null!;
    public virtual ICollection<TicketMessage> TicketMessages { get; set; }


}