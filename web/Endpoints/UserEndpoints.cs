using Application.Common.Models;
using Application.Core.User_core.Commands.CreateUser;
using Application.Core.User_core.Commands.DeleteUser;
using Application.Core.User_core.Commands.UpdateUser;
using Application.Core.User_core.Commands.UserBlockUser;
using Application.Core.User_core.Commands.UserForgetPassword;
using Application.Core.User_core.Commands.UserGoogleLogin;
using Application.Core.User_core.Commands.UserLockUser;
using Application.Core.User_core.Commands.UserLogin;
using Application.Core.User_core.Commands.UserLogoutByAdmin;
using Application.Core.User_core.Commands.UserNotificationLogin;
using Application.Core.User_core.Commands.UserResetPassword;
using Application.Core.User_core.Commands.UserSignUp;
using Application.Core.User_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSwag.Annotations;

namespace Web.Endpoints;

public class User : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(UserFindAll, "FindAll")
            .MapGet(RefreshToken, "RefreshToken")
            .MapPost(GetUserWithPagination, "BindPaging")
            .MapPost(UserOnlineUsers, "UserOnlineUsers")
            .MapPost(CreateUser)
            .MapPost(SignUp, "SignUp")
            .MapPost(UserResetPassword, "UserResetPassword")
            .MapPut(UpdateUser)
            .MapPut(UserLogoutByAdmin,"UserLogoutByAdmin")
            .MapPut(UserLockUser,"UserLockUser")
            .MapPut(UserBlockUser,"UserBlockUser")
            .MapGet(GetUserById, "{Guid}")
            .MapGet(UserLogout, "UserLogout")
            .MapGet(GetERPDashboard, "GetERPDashboard")
            .MapDelete(DeleteUser, "{Guid}")
            .MapPost(Login, "Login")
            .MapPost(UserNotificationLoginCommand, "UserNotificationLoginCommand")
            .MapPost(GoogleLogin, "GoogleLogin")
            .MapPost(UserForgetPassword, "UserForgetPassword")
            ;
    }
    
    [OpenApiOperation("", "ورود به سیستم اطلاع رسانی")]
    public async Task<Ok<Token>> UserNotificationLoginCommand(ISender sender)
    {
        var Response = await sender.Send(new UserNotificationLoginCommand());
        return TypedResults.Ok(Response);
    }

    
    [OpenApiOperation("", "فراموشی رمز عبور")]
    public async Task<Results<NoContent, BadRequest>> UserForgetPassword(ISender sender,
        UserForgetPasswordCommand command)
    {
        await sender.Send(command);
        return TypedResults.NoContent();
    }
    
    [OpenApiOperation("", "نمایش صفحه بندی شده ی لیست کاربران آنلاین")]
    public async Task<Ok<PaginatedList<UserOnlineUsersDTO>>> UserOnlineUsers(ISender sender,
        UserOnlineUsersQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    
    [OpenApiOperation("", "ویرایش قفل کاربر")]
    public async Task<Ok<UserLogoutDTO>> UserLogoutByAdmin(ISender sender, UserLogoutByAdminCommand command)
    {
        var user = await sender.Send(command);

        return TypedResults.Ok(user);
    }
    
    [OpenApiOperation("", "ویرایش قفل کاربر")]
    public async Task<Results<NoContent, BadRequest>> UserLockUser(ISender sender, UserLockUserCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }
    [OpenApiOperation("", "ویرایش بلاک کاربر")]
    public async Task<Results<NoContent, BadRequest>> UserBlockUser(ISender sender, UserBlockUserCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "خروج از سیستم")]
    public async Task<Ok<UserLogoutDTO>> UserLogout(ISender sender)
    {
        var Response = await sender.Send(new UserLogoutQuery());
        return TypedResults.Ok(Response);
    }
   

    

    [OpenApiOperation("", "دریافت داشبورد پنل بولوت")]
    public async Task<Ok<UserGetERPDashboardDTO>> GetERPDashboard(ISender sender, Guid Guid)
    {
        var Response = await sender.Send(new UserGetERPDashboardQuery(Guid));
        return TypedResults.Ok(Response);
    }

    [OpenApiOperation("", "ثبت نام")]
    public async Task<Ok<Token>> SignUp(ISender sender, UserSignUpCommand command)
    {
        var Token = await sender.Send(command);

        return TypedResults.Ok(Token);
    }
    [OpenApiOperation("", "ورود به سیستم")]
    public async Task<Ok<ResetPasswordResult>> UserResetPassword(ISender sender,
        UserResetPasswordCommand command)
    {
       var data= await sender.Send(command);
       return TypedResults.Ok(data);
    }

    [OpenApiOperation("", "لاگین با گوگل")]
    public async Task<Ok<Token>> GoogleLogin(ISender sender, UserGoogleLoginCommand command)
    {
        var Token = await sender.Send(command);

        return TypedResults.Ok(Token);
    }

    [OpenApiOperation("", "رفرش توکن")]
    public async Task<Ok<Token>> RefreshToken(ISender sender)
    {
        var Response = await sender.Send(new UserRefreshTokenQuery());
        return TypedResults.Ok(Response);
    }

    [OpenApiOperation("", "ورود به سیستم")]
    public async Task<Ok<Token>> Login(ISender sender, UserLoginCommand command)
    {
        var Token = await sender.Send(command);

        return TypedResults.Ok(Token);
    }

    [OpenApiOperation("", "نمایش کامل لیست کاربر ها")]
    public async Task<Ok<List<UserFindAllDTO>>> UserFindAll(ISender sender)
    {
        var result = await sender.Send(new UserFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش صفحه بندی شده ی لیست کاربر ها")]
    public async Task<Ok<PaginatedList<UserWithPaginationDTO>>> GetUserWithPagination(ISender sender,
        UserWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "واکشی کاربر با کلید")]
    public async Task<Ok<UserFindByIdDTO>> GetUserById(ISender sender, Guid Guid)
    {
        var Response = await sender.Send(new UserFindByIdQuery(Guid));
        return TypedResults.Ok(Response);
    }

    [OpenApiOperation("", "ثبت کاربر جدید")]
    public async Task<Created<Guid>> CreateUser(ISender sender, CreateUserCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(User)}/{Guid}", Guid);
    }

    [OpenApiOperation("", "ویرایش کاربر")]
    public async Task<Results<NoContent, BadRequest>> UpdateUser(ISender sender, UpdateUserCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "حذف کاربر")]
    public async Task<NoContent> DeleteUser(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteUserCommand(Guid));

        return TypedResults.NoContent();
    }
}