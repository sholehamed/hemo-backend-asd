
namespace Application.Core.OTP_core.Commands.DeleteOTP;

public record DeleteOTPCommand(Guid GUID) : IRequest;

public class DeleteOTPCommandHandler : IRequestHandler<DeleteOTPCommand>
{
    private OTPService OTPService;

    public DeleteOTPCommandHandler(OTPService OTPService)
    {
        this.OTPService = OTPService;
    }

    public async Task Handle(DeleteOTPCommand request, CancellationToken cancellationToken)
    {
        var entity = await OTPService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        OTPService.Table.Remove(entity);

        await OTPService.SaveChangesAsync(cancellationToken);
    }

}

