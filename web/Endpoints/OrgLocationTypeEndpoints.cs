
using Application.Common.Models;
using Application.Core.OrgLocationType_core.Commands.CreateOrgLocationType;
using Application.Core.OrgLocationType_core.Commands.UpdateOrgLocationType;
using Application.Core.OrgLocationType_core.Commands.DeleteOrgLocationType;
using Application.Core.OrgLocationType_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class OrgLocationType : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(OrgLocationTypeFindAll,"FindAll")
            .MapPost(GetOrgLocationTypeWithPagination,"BindPaging")
            .MapPost(CreateOrgLocationType)
            .MapPut(UpdateOrgLocationType)
            .MapGet(GetOrgLocationTypeById, "{Guid}")
            .MapDelete(DeleteOrgLocationType, "{Guid}")
              ;
    }

    [OpenApiOperation( "","نمایش کامل لیست نوع شرکت ها")]
    public async Task<Ok<List<OrgLocationTypeFindAllDTO>>> OrgLocationTypeFindAll(ISender sender)
    {
        var result = await sender.Send(new OrgLocationTypeFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست نوع شرکت ها")]
    public async Task<Ok<PaginatedList<OrgLocationTypeWithPaginationDTO>>> GetOrgLocationTypeWithPagination(ISender sender,  OrgLocationTypeWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی نوع شرکت با کلید")]
    public async Task<Ok<OrgLocationTypeFindByIdDTO>> GetOrgLocationTypeById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new OrgLocationTypeFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت نوع شرکت جدید")]
    public async Task<Created<Guid>> CreateOrgLocationType(ISender sender, CreateOrgLocationTypeCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(OrgLocationType)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش نوع شرکت")]
    public async Task<Results<NoContent, BadRequest>> UpdateOrgLocationType(ISender sender, UpdateOrgLocationTypeCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف نوع شرکت")]
    public async Task<NoContent> DeleteOrgLocationType(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteOrgLocationTypeCommand(Guid));

        return TypedResults.NoContent();
    }
}

