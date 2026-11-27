using Application.Common.Models;
using Application.Core.Api_core.Commands.ApiUpateApiList;
using Application.Core.Api_core.Commands.ApiUpdateInternet;
using Application.Core.Api_core.Commands.ApiUpdateIntranet;
using Application.Core.Api_core.Commands.ApiUpdateIsActive;
using Application.Core.Api_core.Commands.ApiUpdateIsPublic;
using Application.Core.Api_core.Commands.ApiUpdateLogData;
using Application.Core.Api_core.Commands.CreateApi;
using Application.Core.Api_core.Commands.DeleteApi;
using Application.Core.Api_core.Commands.UpdateApi;
using Application.Core.Api_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;

public class Api : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(ApiFindAll, "FindAll")
            .MapGet(ApiLoadBySubsystem, "ApiLoadBySubsystem")
            .MapPost(GetApiWithPagination, "BindPaging")
            .MapPost(UpateApiList, "UpateApiList")
            .MapPost(CreateApi)
            .MapPut(UpdateApi)
            // .MapPut(UpdateIsActive,"UpdateIsActive")
            // .MapPut(ApiUpdateInternet,"ApiUpdateInternet")
            // .MapPut(ApiUpdateIntranet,"ApiUpdateIntranet")
            // .MapPut(ApiUpdateLogData,"ApiUpdateLogData")
            // .MapPut(UpdateIsPublic,"UpdateIsPublic")
            .MapGet(GetApiById, "{Guid}")
            .MapDelete(DeleteApi, "{Guid}")
            ;
    }
    
    [OpenApiOperation("", "نمایش کامل لیست سرویس ها بر اساس سیستم")]
    public async Task<Ok<List<ApiLoadBySubsystemDTO>>> ApiLoadBySubsystem(ISender sender,Guid SubSystemGUID)
    {
        var result = await sender.Send(new ApiLoadBySubsystemQuery(SubSystemGUID));
        return TypedResults.Ok(result);
    }
    
    [OpenApiOperation("", "ویرایش فعال سازی لاگ")]
    public async Task<Results<NoContent, BadRequest>> ApiUpdateLogData(ISender sender, ApiUpdateLogDataCommand command)
    {
        await sender.Send(command);
        return TypedResults.NoContent();
    }
    [OpenApiOperation("", "ویرایش نمایش در اینترنت")]
    public async Task<Results<NoContent, BadRequest>> ApiUpdateInternet(ISender sender, ApiUpdateInternetCommand command)
    {
        await sender.Send(command);
        return TypedResults.NoContent();
    }
    [OpenApiOperation("", "ویرایش نمایش در اینترانت")]
    public async Task<Results<NoContent, BadRequest>> ApiUpdateIntranet(ISender sender, ApiUpdateIntranetCommand command)
    {
        await sender.Send(command);
        return TypedResults.NoContent();
    }
    
    [OpenApiOperation("", "فعال / غیر فعال سازی سرویس")]
    public async Task<Results<NoContent, BadRequest>> UpdateIsActive(ISender sender, ApiUpdateIsActiveCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }
    [OpenApiOperation("", "ویرایش نوع عمومی بودن سرویس")]
    public async Task<Results<NoContent, BadRequest>> UpdateIsPublic(ISender sender, ApiUpdateIsPublicCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "بروز رسانی لیست سرویس ها")]
    public async Task<Created<Guid>> UpateApiList(ISender sender, ApiUpateApiListCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Api)}/{Guid}", Guid);
    }

    [OpenApiOperation("", "نمایش کامل لیست سرویس ها")]
    public async Task<Ok<List<ApiFindAllDTO>>> ApiFindAll(ISender sender)
    {
        var result = await sender.Send(new ApiFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش صفحه بندی شده ی لیست سرویس ها")]
    public async Task<Ok<PaginatedList<ApiWithPaginationDTO>>> GetApiWithPagination(ISender sender,
        ApiWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "واکشی سرویس با کلید")]
    public async Task<Ok<ApiFindByIdDTO>> GetApiById(ISender sender, Guid Guid)
    {
        var Response = await sender.Send(new ApiFindByIdQuery(Guid));
        return TypedResults.Ok(Response);
    }

    [OpenApiOperation("", "ثبت سرویس جدید")]
    public async Task<Created<Guid>> CreateApi(ISender sender, CreateApiCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Api)}/{Guid}", Guid);
    }

    [OpenApiOperation("", "ویرایش سرویس")]
    public async Task<Results<NoContent, BadRequest>> UpdateApi(ISender sender, UpdateApiCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "حذف سرویس")]
    public async Task<NoContent> DeleteApi(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteApiCommand(Guid));

        return TypedResults.NoContent();
    }
}