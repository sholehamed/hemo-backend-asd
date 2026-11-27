
using Application.Common.Models;
using Application.Core.BlockListIP_core.Commands.CreateBlockListIP;
using Application.Core.BlockListIP_core.Commands.UpdateBlockListIP;
using Application.Core.BlockListIP_core.Commands.DeleteBlockListIP;
using Application.Core.BlockListIP_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class BlockListIP : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(BlockListIPFindAll,"FindAll")
            .MapPost(GetBlockListIPWithPagination,"BindPaging")
            .MapPost(CreateBlockListIP)
           // .MapPut(UpdateBlockListIP)
            .MapGet(GetBlockListIPById, "{Guid}")
            .MapDelete(DeleteBlockListIP, "{Guid}")
              ;
    }

    [OpenApiOperation( "","نمایش کامل لیست لیست ای پی عا بلاک شده ها")]
    public async Task<Ok<List<BlockListIPFindAllDTO>>> BlockListIPFindAll(ISender sender)
    {
        var result = await sender.Send(new BlockListIPFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست لیست ای پی عا بلاک شده ها")]
    public async Task<Ok<PaginatedList<BlockListIPWithPaginationDTO>>> GetBlockListIPWithPagination(ISender sender,  BlockListIPWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی لیست ای پی عا بلاک شده با کلید")]
    public async Task<Ok<BlockListIPFindByIdDTO>> GetBlockListIPById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new BlockListIPFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت لیست ای پی عا بلاک شده جدید")]
    public async Task<Created<Guid>> CreateBlockListIP(ISender sender, CreateBlockListIPCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(BlockListIP)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش لیست ای پی عا بلاک شده")]
    public async Task<Results<NoContent, BadRequest>> UpdateBlockListIP(ISender sender, UpdateBlockListIPCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف لیست ای پی عا بلاک شده")]
    public async Task<NoContent> DeleteBlockListIP(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteBlockListIPCommand(Guid));

        return TypedResults.NoContent();
    }
}

