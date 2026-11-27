using Application.Common.Models;
using Application.Core.SubSystem_core.Commands.CreateSubSystem;
using Application.Core.SubSystem_core.Commands.DeleteSubSystem;
using Application.Core.SubSystem_core.Commands.SubSystemUpdateSubSystemIsActive;
using Application.Core.SubSystem_core.Commands.UpdateSubSystem;
using Application.Core.SubSystem_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;

public class SubSystem : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(SubSystemFindAll, "FindAll")
            .MapPost(GetSubSystemWithPagination, "BindPaging")
            .MapPost(CreateSubSystem)
            .MapPut(UpdateSubSystem)
            .MapPut(UpdateSubSystemIsActive,"UpdateSubSystemIsActive")
            .MapGet(GetSubSystemById, "{Guid}")
            .MapDelete(DeleteSubSystem, "{Guid}")
            ;
    }
    
    [OpenApiOperation("", "فعال/غیرفعال سازی زیر سیستم")]
    public async Task<Results<NoContent, BadRequest>> UpdateSubSystemIsActive(ISender sender, SubSystemUpdateSubSystemIsActiveCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "نمایش کامل لیست زیر سیستم ها")]
    public async Task<Ok<List<SubSystemFindAllDTO>>> SubSystemFindAll(ISender sender)
    {
        var result = await sender.Send(new SubSystemFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش صفحه بندی شده ی لیست زیر سیستم ها")]
    public async Task<Ok<PaginatedList<SubSystemWithPaginationDTO>>> GetSubSystemWithPagination(ISender sender,
        SubSystemWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "واکشی زیر سیستم با کلید")]
    public async Task<Ok<SubSystemFindByIdDTO>> GetSubSystemById(ISender sender, Guid Guid)
    {
        var Response = await sender.Send(new SubSystemFindByIdQuery(Guid));
        return TypedResults.Ok(Response);
    }

    [OpenApiOperation("", "ثبت زیر سیستم جدید")]
    public async Task<Created<Guid>> CreateSubSystem(ISender sender, CreateSubSystemCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(SubSystem)}/{Guid}", Guid);
    }

    [OpenApiOperation("", "ویرایش زیر سیستم")]
    public async Task<Results<NoContent, BadRequest>> UpdateSubSystem(ISender sender, UpdateSubSystemCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "حذف زیر سیستم")]
    public async Task<NoContent> DeleteSubSystem(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteSubSystemCommand(Guid));

        return TypedResults.NoContent();
    }
}