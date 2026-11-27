
using Application.Common.Models;
using Application.Core.PasswordBlockList_core.Commands.CreatePasswordBlockList;
using Application.Core.PasswordBlockList_core.Commands.UpdatePasswordBlockList;
using Application.Core.PasswordBlockList_core.Commands.DeletePasswordBlockList;
using Application.Core.PasswordBlockList_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class PasswordBlockList : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(PasswordBlockListFindAll,"FindAll")
            .MapPost(GetPasswordBlockListWithPagination,"BindPaging")
            .MapPost(CreatePasswordBlockList)
            //.MapPut(UpdatePasswordBlockList)
            .MapGet(GetPasswordBlockListById, "{Guid}")
            .MapDelete(DeletePasswordBlockList, "{Guid}")
              ;
    }

    [OpenApiOperation( "","نمایش کامل لیست پسورد های مسدود شده ها")]
    public async Task<Ok<List<PasswordBlockListFindAllDTO>>> PasswordBlockListFindAll(ISender sender)
    {
        var result = await sender.Send(new PasswordBlockListFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست پسورد های مسدود شده ها")]
    public async Task<Ok<PaginatedList<PasswordBlockListWithPaginationDTO>>> GetPasswordBlockListWithPagination(ISender sender,  PasswordBlockListWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی پسورد های مسدود شده با کلید")]
    public async Task<Ok<PasswordBlockListFindByIdDTO>> GetPasswordBlockListById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new PasswordBlockListFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت پسورد های مسدود شده جدید")]
    public async Task<Created<Guid>> CreatePasswordBlockList(ISender sender, CreatePasswordBlockListCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(PasswordBlockList)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش پسورد های مسدود شده")]
    public async Task<Results<NoContent, BadRequest>> UpdatePasswordBlockList(ISender sender, UpdatePasswordBlockListCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف پسورد های مسدود شده")]
    public async Task<NoContent> DeletePasswordBlockList(ISender sender, Guid Guid)
    {
        await sender.Send(new DeletePasswordBlockListCommand(Guid));

        return TypedResults.NoContent();
    }
}

