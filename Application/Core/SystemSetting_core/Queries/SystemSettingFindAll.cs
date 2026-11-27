

namespace Application.Core.SystemSetting_core.Queries;

public record SystemSettingFindAllQuery : IRequest<List<SystemSettingFindAllDTO>>
{

}

public record SystemSettingFindAllDTO : IMapFrom<SystemSetting>
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
        profile.CreateMap<SystemSetting, SystemSettingFindAllDTO>();
    }
}

public class
    SystemSettingFindAllQueryHandler : IRequestHandler<SystemSettingFindAllQuery,
    List<SystemSettingFindAllDTO>>
{
    private SystemSettingService SystemSettingService;
    public SystemSettingFindAllQueryHandler(SystemSettingService SystemSettingService)
    {
        this.SystemSettingService = SystemSettingService;
    }

    public async Task<List<SystemSettingFindAllDTO>> Handle(SystemSettingFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await SystemSettingService.Table.AsNoTracking()
            .ProjectTo<SystemSettingFindAllDTO>(SystemSettingService.mapper.ConfigurationProvider).ToListAsync();
    }
}



