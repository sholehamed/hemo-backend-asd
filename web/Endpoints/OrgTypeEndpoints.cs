
using Application.Common.Models;
using Application.Core.OrgType_core.Commands.CreateOrgType;
using Application.Core.OrgType_core.Commands.UpdateOrgType;
using Application.Core.OrgType_core.Commands.DeleteOrgType;
using Application.Core.OrgType_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class OrgType : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(OrgTypeFindAll,"FindAll")
            .MapPost(GetOrgTypeWithPagination,"BindPaging")
            .MapPost(CreateOrgType)
            .MapPut(UpdateOrgType)
            .MapGet(GetOrgTypeById, "{Guid}")
            .MapDelete(DeleteOrgType, "{Guid}")
              ;
    }

    [OpenApiOperation( "","نمایش کامل لیست دسته بندی نوع شرکت ها")]
    public async Task<Ok<List<OrgTypeFindAllDTO>>> OrgTypeFindAll(ISender sender)
    {
        var result = await sender.Send(new OrgTypeFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست دسته بندی نوع شرکت ها")]
    public async Task<Ok<PaginatedList<OrgTypeWithPaginationDTO>>> GetOrgTypeWithPagination(ISender sender,  OrgTypeWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی دسته بندی نوع شرکت با کلید")]
    public async Task<Ok<OrgTypeFindByIdDTO>> GetOrgTypeById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new OrgTypeFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت دسته بندی نوع شرکت جدید")]
    public async Task<Created<Guid>> CreateOrgType(ISender sender, CreateOrgTypeCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(OrgType)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش دسته بندی نوع شرکت")]
    public async Task<Results<NoContent, BadRequest>> UpdateOrgType(ISender sender, UpdateOrgTypeCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف دسته بندی نوع شرکت")]
    public async Task<NoContent> DeleteOrgType(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteOrgTypeCommand(Guid));

        return TypedResults.NoContent();
    }
}

