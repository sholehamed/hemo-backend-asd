
namespace Application.Core.SystemSetting_core.Commands.UpdateSystemSetting;

public record UpdateSystemSettingCommand : IRequest, IMapFrom<SystemSetting>
{
 
   [PropConfig("اعتبار توکن")]
   public int TokenExpireSeconds { get; init; }
 
   [PropConfig("اعتبار رفرش توکن")]
   public int RefreshTokenExpireSeconds { get; init; }
 
   [PropConfig("تعداد تلاش ناموق جهت ورود")]
   public int LoginAttempts { get; init; }
 
   [PropConfig("مدت زمان مسدود سازی کاربر به دلیل تلاش ناموفق برای ورود")]
   public int LockUserSeconds { get; init; }
 
   [PropConfig("اعتبار توکن")]
   public Guid LoginApiGUID { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }
 
   
   

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateSystemSettingCommand, SystemSetting>();
    }
}

public class UpdateSystemSettingCommandHandler : IRequestHandler<UpdateSystemSettingCommand>
{
    private SystemSettingService SystemSettingService;

    public UpdateSystemSettingCommandHandler(SystemSettingService SystemSettingService)
    {
        this.SystemSettingService = SystemSettingService;
    }

    public async Task Handle(UpdateSystemSettingCommand request, CancellationToken cancellationToken)
    {
        SystemSetting? OldEntity = await SystemSettingService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        SystemSettingService.mapper.Map(request, OldEntity);
        
        await SystemSettingService.SaveChangesAsync(cancellationToken);
    }
}

