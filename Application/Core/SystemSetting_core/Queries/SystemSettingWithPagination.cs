
using Application.Common.Models;

namespace Application.Core.SystemSetting_core.Queries;

public record SystemSettingWithPaginationQuery : PageFilter<SystemSetting, SystemSettingWithPaginationDTO>
{

}

public record SystemSettingWithPaginationDTO : IMapFrom<SystemSetting>
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
        profile.CreateMap<SystemSetting, SystemSettingWithPaginationDTO>();
    }
}

public class
    SystemSettingWithPaginationQueryHandler : IRequestHandler<SystemSettingWithPaginationQuery,
    PaginatedList<SystemSettingWithPaginationDTO>>
{
    private SystemSettingService SystemSettingService;
    public SystemSettingWithPaginationQueryHandler(SystemSettingService SystemSettingService)
    {
        this.SystemSettingService = SystemSettingService;
    }

    public async Task<PaginatedList<SystemSettingWithPaginationDTO>> Handle(SystemSettingWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<SystemSetting> queryable = SystemSettingService.Table;
        return await request.ToPaging( queryable,SystemSettingService.mapper);
    }
}

