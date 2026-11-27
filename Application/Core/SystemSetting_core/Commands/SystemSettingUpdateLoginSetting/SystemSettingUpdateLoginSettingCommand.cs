
namespace Application.Core.SystemSetting_core.Commands.SystemSettingUpdateLoginSetting;

public record SystemSettingUpdateLoginSettingCommand : IRequest, IMapFrom<SystemSetting>
{
    
 
   [PropConfig("تعداد تلاش ناموق جهت ورود")]
   public int LoginAttempts { get; init; }
 
   [PropConfig("مدت زمان مسدود سازی کاربر به دلیل تلاش ناموفق برای ورود")]
   public int LockUserSeconds { get; init; }
   [PropConfig("ورود همزمان مجاز است")]
   public bool MultiLoginAlowed { get; set; }

   [PropConfig("لاگین 2 مرحله ای فعال باشد")]
   public bool TwoStepLogin { get; set; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<SystemSettingUpdateLoginSettingCommand, SystemSetting>();
    }
}

public class SystemSettingUpdateLoginSettingCommandHandler : IRequestHandler<SystemSettingUpdateLoginSettingCommand>
{
    private SystemSettingService SystemSettingService;

    public SystemSettingUpdateLoginSettingCommandHandler(SystemSettingService SystemSettingService)
    {
        this.SystemSettingService = SystemSettingService;
    }

    public async Task Handle(SystemSettingUpdateLoginSettingCommand request, CancellationToken cancellationToken)
    {
        SystemSetting? OldEntity = await SystemSettingService.GetSystemSetting();
        
        SystemSettingService.mapper.Map(request, OldEntity);
        
        await SystemSettingService.SaveChangesAsync(cancellationToken);
    }
}

