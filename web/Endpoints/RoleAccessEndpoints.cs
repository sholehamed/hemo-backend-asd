using Application.Common.Models;
using Application.Core.RoleAccess_core.Commands.CreateRoleAccess;
using Application.Core.RoleAccess_core.Commands.UpdateRoleAccess;
using Application.Core.RoleAccess_core.Commands.DeleteRoleAccess;
using Application.Core.RoleAccess_core.Commands.RoleAccessCheckPagePermission;
using Application.Core.RoleAccess_core.Commands.RoleAccessCopyRoleAccess;
using Application.Core.RoleAccess_core.Commands.RoleAccessUpdateAccessFromOrglocation;
using Application.Core.RoleAccess_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;

public class RoleAccess : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(RoleAccessFindAll, "FindAll")
            .MapPost(GetRoleAccessWithPagination, "BindPaging")
            .MapPost(CreateRoleAccess)
            .MapPost(CheckPagePermission, "CheckPagePermission")
            .MapPut(UpdateRoleAccess)
            .MapPut(UpdateAccessFromOrglocation, "UpdateAccessFromOrglocation")
            .MapPut(CopyRoleAccess, "CopyRoleAccess")
            .MapGet(GetRoleAccessById, "{Guid}")
            .MapDelete(DeleteRoleAccess, "{Guid}")
            ;
    }
    [OpenApiOperation("", "کپی سطخ دسترسی نقش ")]
    public async Task<Results<NoContent, BadRequest>> CopyRoleAccess(ISender sender,
        RoleAccessCopyRoleAccessCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "ویرایش دسترسی نقش توسط شرکت")]
    public async Task<Results<NoContent, BadRequest>> UpdateAccessFromOrglocation(ISender sender,
        RoleAccessUpdateAccessFromOrglocationCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "بررسی دسترسی به صفحه")]
    public async Task<Ok<bool>> CheckPagePermission(ISender sender, RoleAccessCheckPagePermissionCommand command)
    {
        bool Result = await sender.Send(command);
        return TypedResults.Ok(Result);
    }

    [OpenApiOperation("", "نمایش کامل لیست دسترسی نقش ها")]
    public async Task<Ok<List<RoleAccessFindAllDTO>>> RoleAccessFindAll(ISender sender)
    {
        var result = await sender.Send(new RoleAccessFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش صفحه بندی شده ی لیست دسترسی نقش ها")]
    public async Task<Ok<PaginatedList<RoleAccessWithPaginationDTO>>> GetRoleAccessWithPagination(ISender sender,
        RoleAccessWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "واکشی دسترسی نقش با کلید")]
    public async Task<Ok<RoleAccessFindByIdDTO>> GetRoleAccessById(ISender sender, Guid Guid)
    {
        var Response = await sender.Send(new RoleAccessFindByIdQuery(Guid));
        return TypedResults.Ok(Response);
    }

    [OpenApiOperation("", "ثبت دسترسی نقش جدید")]
    public async Task<Created<Guid>> CreateRoleAccess(ISender sender, CreateRoleAccessCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(RoleAccess)}/{Guid}", Guid);
    }

    [OpenApiOperation("", "ویرایش دسترسی نقش")]
    public async Task<Results<NoContent, BadRequest>> UpdateRoleAccess(ISender sender, UpdateRoleAccessCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "حذف دسترسی نقش")]
    public async Task<NoContent> DeleteRoleAccess(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteRoleAccessCommand(Guid));

        return TypedResults.NoContent();
    }
}