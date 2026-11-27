using Domain.Attributes;
namespace Domain.Entities;
[ClassName("پایش ثبت‌شده علائم حیاتی")]
public class BloodTransferVital : AuditedEntity
{
    public Guid BloodTransferGUID { get; set; }
    public Guid ActorUserGUID { get; set; }
    public Guid ActorRoleGUID { get; set; }
    public string Phase { get; set; } = "";
    public DateTime MeasuredAt { get; set; }
    public DateTime RecordedAt { get; set; }
    public int Systolic { get; set; }
    public int Diastolic { get; set; }
    public int Pulse { get; set; }
    public decimal Temperature { get; set; }
    public int Spo2 { get; set; }
    public int Respiration { get; set; }
    public decimal? RateMlPerHour { get; set; }
    public decimal? UrineMl { get; set; }
    public string? Notes { get; set; }
    public virtual BloodTransfer Transfer { get; set; } = null!;
}
