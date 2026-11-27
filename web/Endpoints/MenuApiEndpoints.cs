
using Application.Common.Models;
using Application.Core.MenuApi_core.Commands.CreateMenuApi;
using Application.Core.MenuApi_core.Commands.UpdateMenuApi;
using Application.Core.MenuApi_core.Commands.DeleteMenuApi;
using Application.Core.MenuApi_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class MenuApi : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(MenuApiFindAll,"FindAll")
            .MapPost(GetMenuApiWithPagination,"BindPaging")
            .MapPost(CreateMenuApi)
            .MapPut(UpdateMenuApi)
            .MapGet(GetMenuApiById, "{Guid}")
            .MapGet(BindByMenu, "BindByMenu/{MenuGUID}")
            .MapDelete(DeleteMenuApi, "{Guid}")
              ;
    }
    [OpenApiOperation( "","واکشی سرویس ها با منو")]
    public async Task<Ok<List<MenuApiBindByMenuDTO>>> BindByMenu(ISender sender, Guid MenuGUID)
    {
        var Response=  await sender.Send(new MenuApiBindByMenuQuery(MenuGUID));
        return TypedResults.Ok(Response);
    }
    [OpenApiOperation( "","نمایش کامل لیست سرویس های منو ها")]
    public async Task<Ok<List<MenuApiFindAllDTO>>> MenuApiFindAll(ISender sender)
    {
        var result = await sender.Send(new MenuApiFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست سرویس های منو ها")]
    public async Task<Ok<PaginatedList<MenuApiWithPaginationDTO>>> GetMenuApiWithPagination(ISender sender,  MenuApiWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی سرویس های منو با کلید")]
    public async Task<Ok<MenuApiFindByIdDTO>> GetMenuApiById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new MenuApiFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت سرویس های منو جدید")]
    public async Task<Created<Guid>> CreateMenuApi(ISender sender, CreateMenuApiCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(MenuApi)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش سرویس های منو")]
    public async Task<Results<NoContent, BadRequest>> UpdateMenuApi(ISender sender, UpdateMenuApiCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف سرویس های منو")]
    public async Task<NoContent> DeleteMenuApi(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteMenuApiCommand(Guid));

        return TypedResults.NoContent();
    }
}

