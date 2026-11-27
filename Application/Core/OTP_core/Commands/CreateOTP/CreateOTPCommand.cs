
namespace Application.Core.OTP_core.Commands.CreateOTP;

public record CreateOTPCommand : IRequest<Guid>, IMapFrom<OTP>
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
    
   [PropConfig("کد کانورت دیتا")]
   public string? ConvertID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateOTPCommand, OTP>();
    }
}

public class CreateOTPCommandHandler : IRequestHandler<CreateOTPCommand, Guid>
{
    private OTPService OTPService;
    public CreateOTPCommandHandler(OTPService  OTPService)
    {
        this.OTPService  = OTPService;
    }

    public async Task<Guid> Handle(CreateOTPCommand request, CancellationToken cancellationToken)
    {
        OTP entity = OTPService.mapper.Map<OTP>(request);

        OTPService.Table.Add(entity);

        await OTPService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


