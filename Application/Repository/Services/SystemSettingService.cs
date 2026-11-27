namespace Application.Repository.Services;

public class SystemSettingService : BaseRepository<SystemSetting>
{
    public SystemSettingService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<SystemSetting> GetSystemSetting()
    {
        SystemSetting? SystemSetting = db.SystemSetting.FirstOrDefault();
        if (SystemSetting == null)
        {
            SystemSetting = new SystemSetting()
            {
                TokenExpireSeconds = 600,
                RefreshTokenExpireSeconds = 300,
                LoginAttempts = 3,
                LockUserSeconds = 600,
                MultiLoginAlowed = false,
                LoginApiGUID = db.Api.First(a =>
                    a.DownstreamPathTemplate == "/Identity/User/Login" && a.APITypeName == "post").GUID
            };
            db.SystemSetting.Add(SystemSetting);
            await SaveChangesAsync(CancellationToken.None);
        }

        return SystemSetting;
    }
}