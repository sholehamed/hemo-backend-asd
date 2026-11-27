using Domain.Attributes;

namespace Domain.Entities;

[ClassName("زیر سیستم")]
public class SubSystem : BaseEntity
{
    [PropConfig("عنوان")] public string Name { get; set; }

    [PropConfig("آدرس سرویس")] public string Host { get; set; }

    [PropConfig("پورت سرویس")] public int Port { get; set; }
    
    [PropConfig("فغال")] public bool Active { get; set; }

    [PropConfig("ساعت شروع فعالیت")] public TimeSpan? StartTime { get; set; }
    [PropConfig("ساعت پایان فعالیت")] public TimeSpan? EndTime { get; set; }

  
    public virtual ICollection<Api> Apis { get; set; } = new List<Api>();
}