
using Application.Common.Models;
using Application.Core.UserInvite_core.Commands.CreateUserInvite;
using Application.Core.UserInvite_core.Commands.UpdateUserInvite;
using Application.Core.UserInvite_core.Commands.DeleteUserInvite;
using Application.Core.UserInvite_core.Commands.UserInviteUpdateUserInvite;
using Application.Core.UserInvite_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class UserInvite : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(UserInviteFindAll,"FindAll")
            .MapPost(GetUserInviteWithPagination,"BindPaging")
            .MapPost(UserInviteBindCurrentUserInvites,"BindCurrentUserInvites")
            .MapPost(CreateUserInvite)
            .MapPut(UpdateUserInvite)
            .MapPut(UpdateStatusUserInvite,"UpdateStatusUserInvite")
            .MapGet(GetUserInviteById, "{Guid}")
            .MapDelete(DeleteUserInvite, "{Guid}")
              ;
    }

    [OpenApiOperation( "","ویرایش وضعیت دعوت نامه")]
    public async Task<Results<NoContent, BadRequest>> UpdateStatusUserInvite(ISender sender, UserInviteUpdateUserInviteCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }
    [OpenApiOperation( "","نمایش کامل لیست دعوت نامه ها")]
    public async Task<Ok<List<UserInviteFindAllDTO>>> UserInviteFindAll(ISender sender)
    {
        var result = await sender.Send(new UserInviteFindAllQuery());
        return TypedResults.Ok(result);
    }
    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست دعوت نامه های دریافتی")]
    public async Task<Ok<PaginatedList<UserInviteBindCurrentUserInvitesDTO>>> UserInviteBindCurrentUserInvites(ISender sender,  UserInviteBindCurrentUserInvitesQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست دعوت نامه ها")]
    public async Task<Ok<PaginatedList<UserInviteWithPaginationDTO>>> GetUserInviteWithPagination(ISender sender,  UserInviteWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی دعوت نامه با کلید")]
    public async Task<Ok<UserInviteFindByIdDTO>> GetUserInviteById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new UserInviteFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت دعوت نامه جدید")]
    public async Task<Created<Guid>> CreateUserInvite(ISender sender, CreateUserInviteCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(UserInvite)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش دعوت نامه")]
    public async Task<Results<NoContent, BadRequest>> UpdateUserInvite(ISender sender, UpdateUserInviteCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف دعوت نامه")]
    public async Task<NoContent> DeleteUserInvite(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteUserInviteCommand(Guid));

        return TypedResults.NoContent();
    }
}

