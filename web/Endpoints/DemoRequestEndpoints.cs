
using Application.Common.Models;
using Application.Core.DemoRequest_core.Commands.CreateDemoRequest;
using Application.Core.DemoRequest_core.Commands.UpdateDemoRequest;
using Application.Core.DemoRequest_core.Commands.DeleteDemoRequest;
using Application.Core.DemoRequest_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class DemoRequest : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(DemoRequestFindAll,"FindAll")
            .MapPost(GetDemoRequestWithPagination,"BindPaging")
            .MapPost(CreateDemoRequest)
            .MapPut(UpdateDemoRequest)
            .MapGet(GetDemoRequestById, "{Guid}")
            .MapDelete(DeleteDemoRequest, "{Guid}")
              ;
    }

    [OpenApiOperation( "","نمایش کامل لیست درخواست دمو ها")]
    public async Task<Ok<List<DemoRequestFindAllDTO>>> DemoRequestFindAll(ISender sender)
    {
        var result = await sender.Send(new DemoRequestFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست درخواست دمو ها")]
    public async Task<Ok<PaginatedList<DemoRequestWithPaginationDTO>>> GetDemoRequestWithPagination(ISender sender,  DemoRequestWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی درخواست دمو با کلید")]
    public async Task<Ok<DemoRequestFindByIdDTO>> GetDemoRequestById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new DemoRequestFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت درخواست دمو جدید")]
    public async Task<Created<Guid>> CreateDemoRequest(ISender sender, CreateDemoRequestCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(DemoRequest)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش درخواست دمو")]
    public async Task<Results<NoContent, BadRequest>> UpdateDemoRequest(ISender sender, UpdateDemoRequestCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف درخواست دمو")]
    public async Task<NoContent> DeleteDemoRequest(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteDemoRequestCommand(Guid));

        return TypedResults.NoContent();
    }
}

