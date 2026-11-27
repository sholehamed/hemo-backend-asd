using System.Security.Cryptography;
using Application.Common.Exceptions;
using Application.Repository;
using Application.Repository.Webservice;
using Tool_Utility.Encryption;

namespace Application.Core.User_core.Commands.UserForgetPassword;

public record UserForgetPasswordCommand : IRequest<Guid>, IMapFrom<User>
{
    [PropConfig("نام کاربری", 16)] public string Username { get; init; }
    [PropConfig("شماره موبایل")] public string Mobile { get; init; }

    [PropConfig("رمز عبور")] public string Password { get; init; }
    [PropConfig("تکرار رمز عبور")] public string ConfirmPassword { get; init; }

    [PropConfig("رمز یکبار مصرف")] public string OtpCode { get; init; }
    [PropConfig("UserEnteredCaptchaCode")] public string UserEnteredCaptchaCode { get; init; }
    [PropConfig("CaptchaId")] public string CaptchaId { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserForgetPasswordCommand, User>();
    }
}

public class UserForgetPasswordCommandHandler : IRequestHandler<UserForgetPasswordCommand, Guid>
{
    private readonly UserService UserService;
    private readonly Shahkar Shahkar;
    private readonly SmsService SmsService;

    public UserForgetPasswordCommandHandler(
        UserService userService,
        Shahkar shahkar,
        SmsService smsService)
    {
        UserService = userService;
        Shahkar = shahkar;
        SmsService = smsService;
    }

    public async Task<Guid> Handle(UserForgetPasswordCommand request, CancellationToken cancellationToken)
    {
        UserService.DetectBot(request.UserEnteredCaptchaCode, request.CaptchaId);

        var username = request.Username?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(username))
            throw new ServiceExeption("اطلاعات وارد شده صحیح نمی باشد.");

        var now = DateTime.UtcNow;

        var user = UserService.db.User
            .FirstOrDefault(a => a.Username == username);

        var otpEntity = UserService.db.OTP
            .FirstOrDefault(a =>
                a.Username == username &&
                a.Used == false &&
                a.ExDate >= now);

        // =========================
        // STEP 1 : SEND OTP
        // =========================
        if (string.IsNullOrWhiteSpace(request.OtpCode))
        {
            if (otpEntity != null)
                throw new ServiceExeption("رمز یکبار مصرف قبلا ارسال شده است. لطفا چند دقیقه بعد مجدد تلاش کنید.");

            // جلوگیری از Username Enumeration
            if (user == null)
                return Guid.Empty;
            bool shahkarResult = false;
            if (Int64.Parse(user.Mobile) != long.Parse(request.Mobile))
            {
                shahkarResult = await Shahkar.CheckMobile(user.Nationalcode, request.Mobile);
            }
            else
            {
                shahkarResult = true;
            }

            if (!shahkarResult)
                throw new ServiceExeption("کد ملی و شماره موبایل مطابقت ندارند.");

            user.Mobile = request.Mobile;

            var code = RandomNumberGenerator.GetInt32(100000, 999999);

            var otp = new OTP()
            {
                Code = GenerateOtpHash(user, code.ToString()),
                Username = username,
                ExDate = now.AddMinutes(2),
                Used = false,
                TryCount = 3
            };

            UserService.db.OTP.Add(otp);
            await UserService.SaveChangesAsync(cancellationToken);


            await SmsService.SendOtpMessageAsync(long.Parse(user.Mobile), code );

            return Guid.Empty;
        }

        // =========================
        // STEP 2 : VERIFY OTP
        // =========================

        if (user == null || otpEntity == null)
            throw new ServiceExeption("کد یکبار مصرف صحیح نمی باشد.");

        if (otpEntity.ExDate < now||otpEntity.TryCount <= 0)
        {
            otpEntity.Used = true;
            await UserService.SaveChangesAsync(cancellationToken);
            throw new ServiceExeption("کد امنیتی منقضی شده است.");
        }
        
        var enteredHash = GenerateOtpHash(user, request.OtpCode);

        if (enteredHash != otpEntity.Code)
        {
            otpEntity.TryCount--;
            await UserService.SaveChangesAsync(cancellationToken);
            throw new ServiceExeption("کد یکبار مصرف صحیح نمی باشد.");
        }

        // ===== Password Validation =====

        if (request.Password != request.ConfirmPassword)
            throw new ServiceExeption("رمز عبور و تکرار آن یکسان نمی باشد.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 12)
            throw new ServiceExeption("رمز عبور باید حداقل 12 کاراکتر باشد.");

        bool passwordBlocked = UserService.db.PasswordBlockList
            .Any(a => a.Password == request.Password);

        if (passwordBlocked)
            throw new ServiceExeption("رمز عبور دارای امنیت کافی نمی باشد.");

        user.Password = UserService.HashPassword(user.GUID, request.Password);

        otpEntity.Used = true;

        await UserService.SaveChangesAsync(cancellationToken);

        return user.GUID;
    }

    private string GenerateOtpHash(User user, string code)
    {
        return (user.GUID.ToString() + code + "dzl6QrdRHd7AZU1oeycDqTr1oU")
            .SHA256Hash();
    }
}
