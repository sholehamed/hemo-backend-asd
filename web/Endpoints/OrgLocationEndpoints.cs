using Application.Common.Models;
using Application.Core.OrgLocation_core.Commands.CreateOrgLocation;
using Application.Core.OrgLocation_core.Commands.UpdateOrgLocation;
using Application.Core.OrgLocation_core.Commands.DeleteOrgLocation;
using Application.Core.OrgLocation_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;

public class OrgLocation : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(OrgLocationFindAll, "FindAll")
            .MapGet(FindAllChild, "FindAllChild")
            .MapGet(OrglocationTree, "OrglocationTree")
            .MapPost(GetOrgLocationWithPagination, "BindPaging")
            .MapPost(CreateOrgLocation)
            .MapPut(UpdateOrgLocation)
            .MapGet(GetOrgLocationById, "{Guid}")
            .MapDelete(DeleteOrgLocation, "{Guid}")
            .MapPost(GetSimpleOrgLocationWithPagination, "SimpleBindPaging")
            ;
    }
    
    [OpenApiOperation("", "نمایش کامل لیست شرکت های کاربر جاری")]
    public async Task<Ok<List<OrgLocationFindAllChildDTO>>> FindAllChild(ISender sender)
    {
        var result = await sender.Send(new OrgLocationFindAllChildQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش درختی لیست شرکت ها")]
    public async Task<Ok<List<OrgLocationOrglocationTreeDTO>>> OrglocationTree(ISender sender)
    {
        var result = await sender.Send(new OrgLocationOrglocationTreeQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش کامل لیست شرکت ها")]
    public async Task<Ok<List<OrgLocationFindAllDTO>>> OrgLocationFindAll(ISender sender)
    {
        var result = await sender.Send(new OrgLocationFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش صفحه بندی شده ی لیست شرکت ها")]
    public async Task<Ok<PaginatedList<OrgLocationWithPaginationDTO>>> GetOrgLocationWithPagination(ISender sender,
        OrgLocationWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "واکشی شرکت با کلید")]
    public async Task<Ok<OrgLocationFindByIdDTO>> GetOrgLocationById(ISender sender, Guid Guid)
    {
        var Response = await sender.Send(new OrgLocationFindByIdQuery(Guid));
        return TypedResults.Ok(Response);
    }

    [OpenApiOperation("", "ثبت شرکت جدید")]
    public async Task<Created<Guid>> CreateOrgLocation(ISender sender, CreateOrgLocationCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(OrgLocation)}/{Guid}", Guid);
    }

    [OpenApiOperation("", "ویرایش شرکت")]
    public async Task<Results<NoContent, BadRequest>> UpdateOrgLocation(ISender sender,
        UpdateOrgLocationCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "حذف شرکت")]
    public async Task<NoContent> DeleteOrgLocation(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteOrgLocationCommand(Guid));

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "نمایش ساده  صفحه بندی شده ی لیست شرکت ها")]
    public async Task<Ok<PaginatedList<SimpleOrgLocationWithPaginationDTO>>> GetSimpleOrgLocationWithPagination(ISender sender,
       SimpleOrgLocationWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
}