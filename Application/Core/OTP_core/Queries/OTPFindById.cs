
namespace Application.Core.OTP_core.Queries;

public record OTPFindByIdQuery(Guid GUID) : IRequest<OTPFindByIdDTO>;

public record OTPFindByIdDTO : IMapFrom<OTP>
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
        profile.CreateMap<OTP, OTPFindByIdDTO>();
    }
}

public class
    OTPFindByIdQueryHandler : IRequestHandler<OTPFindByIdQuery, OTPFindByIdDTO>
{
    private OTPService OTPService;

    public OTPFindByIdQueryHandler(OTPService OTPService)
    {
        this.OTPService = OTPService;
    }

    public async Task<OTPFindByIdDTO> Handle(OTPFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        OTPFindByIdDTO? Data = OTPService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<OTPFindByIdDTO>(OTPService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

