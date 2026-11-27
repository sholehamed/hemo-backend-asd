using Domain.Attributes;

namespace Domain.Entities;

[ClassName("پیام")]
public class TicketMessage:AuditedEntity
{
    [PropConfig("تیکت")]
    public Guid TicketGUID { get; set; }

    [PropConfig("متن پیام")]
    public string Message { get; set; }
    
    [PropConfig("پیام از نوع فایل می باشد.")]
    public bool IsFile { get; set; }
    
    [PropConfig("کاربر ارسال کننده")]
    public Guid UserGUID { get; set; }
    
    [PropConfig("تاریخ و ساعت ارسال")]
    public DateTime SendDate { get; set; }
    
    public virtual Ticket Ticket { get; set; } = null!;

}