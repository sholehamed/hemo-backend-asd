
namespace Application.Core.OTP_core.Commands.UpdateOTP;

public record UpdateOTPCommand : IRequest, IMapFrom<OTP>
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
        profile.CreateMap<UpdateOTPCommand, OTP>();
    }
}

public class UpdateOTPCommandHandler : IRequestHandler<UpdateOTPCommand>
{
    private OTPService OTPService;

    public UpdateOTPCommandHandler(OTPService OTPService)
    {
        this.OTPService = OTPService;
    }

    public async Task Handle(UpdateOTPCommand request, CancellationToken cancellationToken)
    {
        OTP? OldEntity = await OTPService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        OTPService.mapper.Map(request, OldEntity);
        
        await OTPService.SaveChangesAsync(cancellationToken);
    }
}

