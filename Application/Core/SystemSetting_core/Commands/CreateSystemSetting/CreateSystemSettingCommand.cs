
namespace Application.Core.SystemSetting_core.Commands.CreateSystemSetting;

public record CreateSystemSettingCommand : IRequest<Guid>, IMapFrom<SystemSetting>
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
    
   
   


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateSystemSettingCommand, SystemSetting>();
    }
}

public class CreateSystemSettingCommandHandler : IRequestHandler<CreateSystemSettingCommand, Guid>
{
    private SystemSettingService SystemSettingService;
    public CreateSystemSettingCommandHandler(SystemSettingService  SystemSettingService)
    {
        this.SystemSettingService  = SystemSettingService;
    }

    public async Task<Guid> Handle(CreateSystemSettingCommand request, CancellationToken cancellationToken)
    {
        SystemSetting entity = SystemSettingService.mapper.Map<SystemSetting>(request);

        SystemSettingService.Table.Add(entity);

        await SystemSettingService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


