using Application.Common.Models;
using Application.Core.UserRole_core.Commands.CreateUserRole;
using Application.Core.UserRole_core.Commands.DeleteUserRole;
using Application.Core.UserRole_core.Commands.UpdateUserRole;
using Application.Core.UserRole_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;

public class UserRole : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(UserRoleFindAll, "FindAll")
            .MapGet(GetCurrentUserRoles, "GetCurrentUserRoles")
            .MapPost(GetUserRoleWithPagination, "BindPaging")
            .MapPost(CreateUserRole)
            .MapPut(UpdateUserRole)
            .MapGet(GetUserRoleById, "{Guid}")
            .MapDelete(DeleteUserRole, "{Guid}")
            ;
    }
    [OpenApiOperation("", "دریافت لیست نقش های کاربر")]
    public async Task<Ok<List<UserRoleGetCurrentUserRolesDTO>>> GetCurrentUserRoles(ISender sender)
    {
        var result = await sender.Send(new UserRoleGetCurrentUserRolesQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش کامل لیست نقش کاربر ها")]
    public async Task<Ok<List<UserRoleFindAllDTO>>> UserRoleFindAll(ISender sender)
    {
        var result = await sender.Send(new UserRoleFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش صفحه بندی شده ی لیست نقش کاربر ها")]
    public async Task<Ok<PaginatedList<UserRoleWithPaginationDTO>>> GetUserRoleWithPagination(ISender sender,
        UserRoleWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "واکشی نقش کاربر با کلید")]
    public async Task<Ok<UserRoleFindByIdDTO>> GetUserRoleById(ISender sender, Guid Guid)
    {
        var Response = await sender.Send(new UserRoleFindByIdQuery(Guid));
        return TypedResults.Ok(Response);
    }

    [OpenApiOperation("", "ثبت نقش کاربر جدید")]
    public async Task<Created<Guid>> CreateUserRole(ISender sender, CreateUserRoleCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(UserRole)}/{Guid}", Guid);
    }

    [OpenApiOperation("", "ویرایش نقش کاربر")]
    public async Task<Results<NoContent, BadRequest>> UpdateUserRole(ISender sender, UpdateUserRoleCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "حذف نقش کاربر")]
    public async Task<NoContent> DeleteUserRole(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteUserRoleCommand(Guid));

        return TypedResults.NoContent();
    }
}