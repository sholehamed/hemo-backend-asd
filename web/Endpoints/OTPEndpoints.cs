
using Application.Common.Models;
using Application.Core.OTP_core.Commands.CreateOTP;
using Application.Core.OTP_core.Commands.UpdateOTP;
using Application.Core.OTP_core.Commands.DeleteOTP;
using Application.Core.OTP_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class OTP : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(OTPFindAll,"FindAll")
            .MapPost(GetOTPWithPagination,"BindPaging")
            .MapPost(CreateOTP)
            .MapPut(UpdateOTP)
            .MapGet(GetOTPById, "{Guid}")
            .MapDelete(DeleteOTP, "{Guid}")
              ;
    }

    [OpenApiOperation( "","نمایش کامل لیست رمز یکبار مصرف ها")]
    public async Task<Ok<List<OTPFindAllDTO>>> OTPFindAll(ISender sender)
    {
        var result = await sender.Send(new OTPFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست رمز یکبار مصرف ها")]
    public async Task<Ok<PaginatedList<OTPWithPaginationDTO>>> GetOTPWithPagination(ISender sender,  OTPWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی رمز یکبار مصرف با کلید")]
    public async Task<Ok<OTPFindByIdDTO>> GetOTPById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new OTPFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت رمز یکبار مصرف جدید")]
    public async Task<Created<Guid>> CreateOTP(ISender sender, CreateOTPCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(OTP)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش رمز یکبار مصرف")]
    public async Task<Results<NoContent, BadRequest>> UpdateOTP(ISender sender, UpdateOTPCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف رمز یکبار مصرف")]
    public async Task<NoContent> DeleteOTP(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteOTPCommand(Guid));

        return TypedResults.NoContent();
    }
}

