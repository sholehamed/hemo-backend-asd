
using Application.Common.Models;
using Application.Core.Whitelisting_core.Commands.CreateWhitelisting;
using Application.Core.Whitelisting_core.Commands.UpdateWhitelisting;
using Application.Core.Whitelisting_core.Commands.DeleteWhitelisting;
using Application.Core.Whitelisting_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class Whitelisting : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(WhitelistingFindAll,"FindAll")
            .MapPost(GetWhitelistingWithPagination,"BindPaging")
            .MapPost(CreateWhitelisting)
            //.MapPut(UpdateWhitelisting)
            .MapGet(GetWhitelistingById, "{Guid}")
            .MapDelete(DeleteWhitelisting, "{Guid}")
              ;
    }

    [OpenApiOperation( "","نمایش کامل لیست لیست ای پی ها مجاز ها")]
    public async Task<Ok<List<WhitelistingFindAllDTO>>> WhitelistingFindAll(ISender sender)
    {
        var result = await sender.Send(new WhitelistingFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست لیست ای پی ها مجاز ها")]
    public async Task<Ok<PaginatedList<WhitelistingWithPaginationDTO>>> GetWhitelistingWithPagination(ISender sender,  WhitelistingWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی لیست ای پی ها مجاز با کلید")]
    public async Task<Ok<WhitelistingFindByIdDTO>> GetWhitelistingById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new WhitelistingFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت لیست ای پی ها مجاز جدید")]
    public async Task<Created<Guid>> CreateWhitelisting(ISender sender, CreateWhitelistingCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Whitelisting)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش لیست ای پی ها مجاز")]
    public async Task<Results<NoContent, BadRequest>> UpdateWhitelisting(ISender sender, UpdateWhitelistingCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف لیست ای پی ها مجاز")]
    public async Task<NoContent> DeleteWhitelisting(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteWhitelistingCommand(Guid));

        return TypedResults.NoContent();
    }
}

