
using Application.Common.Models;

namespace Application.Core.OTP_core.Queries;

public record OTPWithPaginationQuery : PageFilter<OTP, OTPWithPaginationDTO>
{

}

public record OTPWithPaginationDTO : IMapFrom<OTP>
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
        profile.CreateMap<OTP, OTPWithPaginationDTO>();
    }
}

public class
    OTPWithPaginationQueryHandler : IRequestHandler<OTPWithPaginationQuery,
    PaginatedList<OTPWithPaginationDTO>>
{
    private OTPService OTPService;
    public OTPWithPaginationQueryHandler(OTPService OTPService)
    {
        this.OTPService = OTPService;
    }

    public async Task<PaginatedList<OTPWithPaginationDTO>> Handle(OTPWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<OTP> queryable = OTPService.Table;
        return await request.ToPaging( queryable,OTPService.mapper);
    }
}

