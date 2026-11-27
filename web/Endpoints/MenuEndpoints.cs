using Application.Common.Models;
using Application.Core.Menu_core.Commands.CreateMenu;
using Application.Core.Menu_core.Commands.DeleteMenu;
using Application.Core.Menu_core.Commands.MenuUpdateMenuIsPublic;
using Application.Core.Menu_core.Commands.MenuUpdateTree;
using Application.Core.Menu_core.Commands.MenuUpdateVisibility;
using Application.Core.Menu_core.Commands.UpdateMenu;
using Application.Core.Menu_core.Queries;
using Application.Core.Role_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;

public class Menu : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(MenuBindUserMenu, "MenuBindUserMenu")
            .MapGet(MenuForAccess, "MenuForAccess/{OrglocationPackageGUID}/{RoleGUID}")
            .MapGet(MenuFindAll, "FindAll")
            .MapPost(GetMenuWithPagination, "BindPaging")
            .MapPost(CreateMenu)
            .MapPut(UpdateMenu)
            .MapGet(MenuGetAllRoute, "MenuGetAllRoute")
            .MapPut(UpdateVisibility, "UpdateVisibility")
            .MapPut(UpdateMenuIsPublic, "UpdateMenuIsPublic")
            .MapPut(MenuUpdateTree, "MenuUpdateTree")
            .MapGet(GetMenuById, "{Guid}")
            .MapGet(MenuBindTreeMenu, "MenuBindTreeMenu")
            .MapDelete(DeleteMenu, "{Guid}")
            ;
    }
    [OpenApiOperation("", "نمایش کامل لیست منو ها")]
    public async Task<Ok<List<MenuGetAllRouteDTO>>> MenuGetAllRoute(ISender sender)
    {
        var result = await sender.Send(new MenuGetAllRouteQuery());
        return TypedResults.Ok(result);
    }
    [OpenApiOperation("", "دریافت لیست دسترسی های پکیج")]
    public async Task<Ok<List<MenuMenuForAccessDTO>>> MenuForAccess(ISender sender, Guid OrglocationPackageGUID,
        Guid RoleGUID)
    {
        var result = await sender.Send(new MenuMenuForAccessQuery(OrglocationPackageGUID, RoleGUID));
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش درختی لیست منو ها")]
    public async Task<Ok<List<MenuBindTreeMenuDTO>>> MenuBindTreeMenu(ISender sender)
    {
        var result = await sender.Send(new MenuBindTreeMenuQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "ویرایش ترتیب درخت منو")]
    public async Task<Results<NoContent, BadRequest>> MenuUpdateTree(ISender sender, MenuUpdateTreeCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "ویرایش نمایش منو")]
    public async Task<Results<NoContent, BadRequest>> UpdateVisibility(ISender sender,
        MenuUpdateVisibilityCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "ویرایش عمومی بودن منو")]
    public async Task<Results<NoContent, BadRequest>> UpdateMenuIsPublic(ISender sender,
        MenuUpdateMenuIsPublicCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }


    [OpenApiOperation("", "دریافت منو های کاربر جاری")]
    public async Task<Ok<List<MenuBindUserMenuDTO>>> MenuBindUserMenu(ISender sender, string SubPath)
    {
        var result = await sender.Send(new MenuBindUserMenuQuery(SubPath));
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش کامل لیست منو ها")]
    public async Task<Ok<List<MenuFindAllDTO>>> MenuFindAll(ISender sender)
    {
        var result = await sender.Send(new MenuFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش صفحه بندی شده ی لیست منو ها")]
    public async Task<Ok<PaginatedList<MenuWithPaginationDTO>>> GetMenuWithPagination(ISender sender,
        MenuWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "واکشی منو با کلید")]
    public async Task<Ok<MenuFindByIdDTO>> GetMenuById(ISender sender, Guid Guid)
    {
        var Response = await sender.Send(new MenuFindByIdQuery(Guid));
        return TypedResults.Ok(Response);
    }

    [OpenApiOperation("", "ثبت منو جدید")]
    public async Task<Created<Guid>> CreateMenu(ISender sender, CreateMenuCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Menu)}/{Guid}", Guid);
    }

    [OpenApiOperation("", "ویرایش منو")]
    public async Task<Results<NoContent, BadRequest>> UpdateMenu(ISender sender, UpdateMenuCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "حذف منو")]
    public async Task<NoContent> DeleteMenu(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteMenuCommand(Guid));

        return TypedResults.NoContent();
    }
}