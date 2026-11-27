
using Application.Common.Models;
using Application.Core.Priority_core.Commands.CreatePriority;
using Application.Core.Priority_core.Commands.UpdatePriority;
using Application.Core.Priority_core.Commands.DeletePriority;
using Application.Core.Priority_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class Priority : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(PriorityFindAll,"FindAll")
            .MapPost(GetPriorityWithPagination,"BindPaging")
            .MapPost(CreatePriority)
            .MapPut(UpdatePriority)
            .MapGet(GetPriorityById, "{Guid}")
            .MapDelete(DeletePriority, "{Guid}")
              ;
    }

    [OpenApiOperation( "","نمایش کامل لیست اولویت ها")]
    public async Task<Ok<List<PriorityFindAllDTO>>> PriorityFindAll(ISender sender)
    {
        var result = await sender.Send(new PriorityFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست اولویت ها")]
    public async Task<Ok<PaginatedList<PriorityWithPaginationDTO>>> GetPriorityWithPagination(ISender sender,  PriorityWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی اولویت با کلید")]
    public async Task<Ok<PriorityFindByIdDTO>> GetPriorityById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new PriorityFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت اولویت جدید")]
    public async Task<Created<Guid>> CreatePriority(ISender sender, CreatePriorityCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Priority)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش اولویت")]
    public async Task<Results<NoContent, BadRequest>> UpdatePriority(ISender sender, UpdatePriorityCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف اولویت")]
    public async Task<NoContent> DeletePriority(ISender sender, Guid Guid)
    {
        await sender.Send(new DeletePriorityCommand(Guid));

        return TypedResults.NoContent();
    }
}

