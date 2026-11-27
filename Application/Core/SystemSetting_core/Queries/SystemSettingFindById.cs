namespace Application.Core.SystemSetting_core.Queries;

public record SystemSettingFindByIdQuery : IRequest<SystemSettingFindByIdDTO>;

public record SystemSettingFindByIdDTO : IMapFrom<SystemSetting>
{
    [PropConfig("اعتبار توکن")] public int TokenExpireSeconds { get; init; }

    [PropConfig("اعتبار رفرش توکن")] public int RefreshTokenExpireSeconds { get; init; }

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
        profile.CreateMap<SystemSetting, SystemSettingFindByIdDTO>();
    }
}

public class
    SystemSettingFindByIdQueryHandler : IRequestHandler<SystemSettingFindByIdQuery, SystemSettingFindByIdDTO>
{
    private SystemSettingService SystemSettingService;

    public SystemSettingFindByIdQueryHandler(SystemSettingService SystemSettingService)
    {
        this.SystemSettingService = SystemSettingService;
    }

    public async Task<SystemSettingFindByIdDTO> Handle(SystemSettingFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        SystemSetting systemSetting = await SystemSettingService.GetSystemSetting();
        SystemSettingFindByIdDTO Data = new SystemSettingFindByIdDTO()
        {
            LoginAttempts = systemSetting.LoginAttempts,
            LockUserSeconds = systemSetting.LockUserSeconds,
            TokenExpireSeconds = systemSetting.TokenExpireSeconds,
            RefreshTokenExpireSeconds = systemSetting.RefreshTokenExpireSeconds,
            MultiLoginAlowed= systemSetting.MultiLoginAlowed,
            TwoStepLogin = systemSetting.TwoStepLogin

        };

        return Data;
    }
}