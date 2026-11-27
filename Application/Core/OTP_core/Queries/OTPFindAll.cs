

namespace Application.Core.OTP_core.Queries;

public record OTPFindAllQuery : IRequest<List<OTPFindAllDTO>>
{

}

public record OTPFindAllDTO : IMapFrom<OTP>
{

    [PropConfig("نام کاربری")]
    public string Username { get; init; }

    [PropConfig("کد یکبار مصرف")]
    public string Code { get; init; }

    [PropConfig("تاریخ انقضا")]
    public DateTime ExDate { get; init; }

    [PropConfig("تعداد تلاش مجاز")]
    public int TryCount { get; init; }

    [PropConfig("استفاده شده")]
    public bool Used { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }

    [PropConfig("کد کانورت دیتا")]
    public string? ConvertID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<OTP, OTPFindAllDTO>();
    }
}

public class
    OTPFindAllQueryHandler : IRequestHandler<OTPFindAllQuery,
    List<OTPFindAllDTO>>
{
    private OTPService OTPService;
    public OTPFindAllQueryHandler(OTPService OTPService)
    {
        this.OTPService = OTPService;
    }

    public async Task<List<OTPFindAllDTO>> Handle(OTPFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await OTPService.Table.AsNoTracking()
            .ProjectTo<OTPFindAllDTO>(OTPService.mapper.ConfigurationProvider).ToListAsync();
    }
}



