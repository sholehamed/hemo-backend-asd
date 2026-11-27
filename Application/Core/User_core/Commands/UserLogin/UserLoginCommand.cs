using System.Security.Cryptography;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Repository.Webservice;
using Tool_Utility.Encryption;

namespace Application.Core.User_core.Commands.UserLogin;

public record UserLoginCommand : IRequest<Token>, IMapFrom<User>
{
    [PropConfig("نام کاربری", 16)] public string Username { get; init; }

    [PropConfig("رمز عبور")] public string Password { get; init; }
    [PropConfig("UserEnteredCaptchaCode")] public string UserEnteredCaptchaCode { get; init; }
    [PropConfig("CaptchaId")] public string CaptchaId { get; init; }

    [PropConfig("OtpCode")] public string? OtpCode { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserLoginCommand, User>();
    }
}

public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, Token>
{
    private readonly UserService UserService;
    private readonly SmsService SmsService;
    private SystemSettingService SystemSettingService;

    public UserLoginCommandHandler(UserService userService, SmsService smsService,
        SystemSettingService systemSettingService)
    {
        UserService = userService;
        SmsService = smsService;
        SystemSettingService = systemSettingService;
    }

    public async Task<Token> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        UserService.DetectBot(request.UserEnteredCaptchaCode, request.CaptchaId);

        SystemSetting systemSetting = await SystemSettingService.GetSystemSetting();

        var errorMessage = "نام کاربری یا رمز عبور اشتباه می باشد.";
        var username = request.Username?.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(username))
            throw new ServiceExeption(errorMessage);

        var user = UserService.db.User
            .FirstOrDefault(a => a.Username.ToLower() == username);

        await UserService.CheckUserLoginAccess(user);

        if (user == null || !PasswordIsValid(user, request.Password))
        {
            if (user != null)
                await UserService.OnFaildLogin(user.GUID);

            throw new ServiceExeption(errorMessage);
        }

        if (systemSetting.TwoStepLogin == true || user.MobileIsConfirm == false)
        {
            var now = DateTime.UtcNow;

            var otpEntity = UserService.db.OTP
                .FirstOrDefault(a =>
                    a.Username == user.Username.ToLower() &&
                    a.Used == false &&
                    a.ExDate >= now);

            // ====== STEP 1 : SEND OTP ======
            if (string.IsNullOrWhiteSpace(request.OtpCode))
            {
                if (otpEntity != null)
                    throw new ServiceExeption("رمز یکبار مصرف قبلا ارسال شده است. لطفا چند دقیقه بعد مجدد تلاش کنید.");


                if (string.IsNullOrWhiteSpace(user.Mobile) || !user.Mobile.StartsWith("09") || user.Mobile.Length != 11)
                    throw new ServiceExeption(
                        "شماره موبایل ثبت نشده است. از راه فراموشی رمز عبور می توانید شماره موبایل جدید تنظیم کنید.");

                var code = RandomNumberGenerator.GetInt32(100000, 999999);

                var otp = new OTP()
                {
                    Code = GenerateOtpHash(user, code.ToString()),
                    Username = user.Username.ToLower(),
                    ExDate = DateTime.UtcNow.AddMinutes(2),
                    Used = false,
                    TryCount = 3
                };

                UserService.db.OTP.Add(otp);
                await UserService.SaveChangesAsync(cancellationToken);


                await SmsService.SendOtpMessageAsync(long.Parse(user.Mobile), code);

                return new Token { Key = "otp" };
            }

            // ====== STEP 2 : VERIFY OTP ======

            if (otpEntity == null)
                throw new ServiceExeption("کد یکبار مصرف صحیح نمی باشد.");

            if (otpEntity.TryCount <= 0)
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

            otpEntity.Used = true;
            await UserService.SaveChangesAsync(cancellationToken);

            await UserService.OnSuccessLogin(user.GUID);

            return await UserService.GetToken(user, null);
        }
        else
        {
            await UserService.OnSuccessLogin(user.GUID);
            return await UserService.GetToken(user, null);
        }
    }


    public bool PasswordIsValid(User user, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        return user.Password ==
               UserService.HashPassword(user.GUID, password);
    }

    private string GenerateOtpHash(User user, string code)
    {
        return (user.GUID.ToString() + code + "dzl6QrdRHd7AZU1oeycDqTr1oU")
            .SHA256Hash();
    }
}