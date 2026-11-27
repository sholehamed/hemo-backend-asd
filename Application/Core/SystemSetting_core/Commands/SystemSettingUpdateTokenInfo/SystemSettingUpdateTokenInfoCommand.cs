
namespace Application.Core.SystemSetting_core.Commands.SystemSettingUpdateTokenInfo;

public record SystemSettingUpdateTokenInfoCommand : IRequest, IMapFrom<SystemSetting>
{
 
   [PropConfig("اعتبار توکن")]
   public int TokenExpireSeconds { get; init; }
 
   [PropConfig("اعتبار رفرش توکن")]
   public int RefreshTokenExpireSeconds { get; init; }
 
   
    public void Mapping(Profile profile)
    {
        profile.CreateMap<SystemSettingUpdateTokenInfoCommand, SystemSetting>();
    }
}

public class SystemSettingUpdateTokenInfoCommandHandler : IRequestHandler<SystemSettingUpdateTokenInfoCommand>
{
    private SystemSettingService SystemSettingService;

    public SystemSettingUpdateTokenInfoCommandHandler(SystemSettingService SystemSettingService)
    {
        this.SystemSettingService = SystemSettingService;
    }

    public async Task Handle(SystemSettingUpdateTokenInfoCommand request, CancellationToken cancellationToken)
    {
        SystemSetting? OldEntity = await SystemSettingService.GetSystemSetting();

        
        SystemSettingService.mapper.Map(request, OldEntity);
        
        await SystemSettingService.SaveChangesAsync(cancellationToken);
    }
}

