using Application.Common.Models;
using Application.Core.Role_core.Commands.CreateRole;
using Application.Core.Role_core.Commands.DeleteRole;
using Application.Core.Role_core.Commands.UpdateRole;
using Application.Core.Role_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;

public class Role : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(RoleFindAll, "FindAll")
            .MapGet(RoleFindAllTree, "RoleFindAllTree")
            .MapPost(GetRoleWithPagination, "BindPaging")
            .MapPost(CreateRole)
            .MapPut(UpdateRole)
            .MapGet(GetRoleById, "{Guid}")
            .MapDelete(DeleteRole, "{Guid}")
            ;
    }
    [OpenApiOperation("", "نمایش درختی لیست نقش ها")]
    public async Task<Ok<List<RoleFindAllTreeDTO>>> RoleFindAllTree(ISender sender)
    {
        var result = await sender.Send(new RoleFindAllTreeQuery());
        return TypedResults.Ok(result);
    }
    [OpenApiOperation("", "نمایش کامل لیست نقش ها")]
    public async Task<Ok<List<RoleFindAllDTO>>> RoleFindAll(ISender sender)
    {
        var result = await sender.Send(new RoleFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش صفحه بندی شده ی لیست نقش ها")]
    public async Task<Ok<PaginatedList<RoleWithPaginationDTO>>> GetRoleWithPagination(ISender sender,
        RoleWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "واکشی نقش با کلید")]
    public async Task<Ok<RoleFindByIdDTO>> GetRoleById(ISender sender, Guid Guid)
    {
        var Response = await sender.Send(new RoleFindByIdQuery(Guid));
        return TypedResults.Ok(Response);
    }

    [OpenApiOperation("", "ثبت نقش جدید")]
    public async Task<Created<Guid>> CreateRole(ISender sender, CreateRoleCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Role)}/{Guid}", Guid);
    }

    [OpenApiOperation("", "ویرایش نقش")]
    public async Task<Results<NoContent, BadRequest>> UpdateRole(ISender sender, UpdateRoleCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "حذف نقش")]
    public async Task<NoContent> DeleteRole(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteRoleCommand(Guid));

        return TypedResults.NoContent();
    }
}