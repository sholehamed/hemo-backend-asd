using Application.Common.Models;
using Application.Repository.Webservice;

namespace Application.Core.User_core.Commands.UserSignUp;

public record UserSignUpCommand : IRequest<Token>, IMapFrom<User>
{
    [PropConfig("نام کاربری", MaxLenght: 64)]
    public string? Username { get; init; }
    [PropConfig("تاریخ تولد")]
    public DateTime BirthDate { get; init; }
    [PropConfig("رمز عبور")] public string Password { get; init; }

    [PropConfig("موبایل", MaxLenght: 16)] public string? Mobile { get; init; }

    [PropConfig("ایمیل", MaxLenght: 64)] public string? Email { get; init; }

    [PropConfig("UserEnteredCaptchaCode")] public string UserEnteredCaptchaCode { get; init; }
    [PropConfig("CaptchaId")] public string CaptchaId { get; init; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserSignUpCommand, User>();
    }
}

public class UserSignUpCommandHandler : IRequestHandler<UserSignUpCommand, Token>
{
    private UserService UserService;
    private Sabtahval Sabtahval;

    public UserSignUpCommandHandler(UserService userService, Sabtahval sabtahval)
    {
        UserService = userService;
        Sabtahval = sabtahval;
    }

    public async Task<Token> Handle(UserSignUpCommand request, CancellationToken cancellationToken)
    {
        UserService.DetectBot(request.UserEnteredCaptchaCode, request.CaptchaId);

        PersonInfo personInfo = await Sabtahval.GetUserInfo(request.Username, request.BirthDate);
   
        var result = await UserService.CreateUser(request.Username, request.Password, personInfo.Name,
            personInfo.Family,
            request.Email, request.Mobile, Guid.Parse("0A5E5228-9C6C-4B81-BCA4-9BE6363C7AAE"),
            $"پروفایل شخصی {personInfo.Name} {personInfo.Family}",
            "", "", "", "", "", DateTime.Now.AddYears(100),
            true,request.Username,
            cancellationToken,personInfo.FatherName,personInfo.IsMan);

        return new Token();
    }
}