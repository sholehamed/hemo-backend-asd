using Application.Common.Models;
using Application.Core.OrglocationPackage_core.Commands.CreateOrglocationPackage;
using Application.Core.OrglocationPackage_core.Commands.UpdateOrglocationPackage;
using Application.Core.OrglocationPackage_core.Commands.DeleteOrglocationPackage;
using Application.Core.OrglocationPackage_core.Commands.OrglocationPackageBuyPack;
using Application.Core.OrglocationPackage_core.Commands.OrglocationPackageUpdateExpireDate;
using Application.Core.OrglocationPackage_core.Commands.OrglocationPackageUpdateVisibility;
using Application.Core.OrglocationPackage_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;

public class OrglocationPackage : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(OrglocationPackageFindAll, "FindAll")
            .MapGet(GetCurrentUserPacks, "GetCurrentUserPacks")
            .MapPost(GetOrglocationPackageWithPagination, "BindPaging")
            .MapPost(OrglocationPackageGetByCurrentUser, "OrglocationPackageGetByCurrentUser")
            .MapPost(CreateOrglocationPackage)
            .MapPost(BuyPack, "BuyPack")
            .MapPut(UpdateOrglocationPackage)
            .MapPut(OrglocationPackageUpdateExpireDate, "OrglocationPackageUpdateExpireDate")
            .MapPut(OrglocationPackageUpdateVisibility, "OrglocationPackageUpdateVisibility")
            .MapGet(GetOrglocationPackageById, "{Guid}")
            .MapDelete(DeleteOrglocationPackage, "{Guid}")
            ;
    }

    [OpenApiOperation("", "ویرایش پنهان کردن پکیج شرکت")]
    public async Task<Results<NoContent, BadRequest>> OrglocationPackageUpdateVisibility(ISender sender,
        OrglocationPackageUpdateVisibilityCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }
    

    
    [OpenApiOperation("", "نمایش کل  پکیج های شرکت ")]
    public async Task<Ok<PaginatedList<OrglocationPackageGetByCurrentUserDTO>>> OrglocationPackageGetByCurrentUser(
        ISender sender, OrglocationPackageGetByCurrentUserQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }
    
    [OpenApiOperation("", "ویرایش اعتبار پکیج های شرکت")]
    public async Task<Results<NoContent, BadRequest>> OrglocationPackageUpdateExpireDate(ISender sender,
        OrglocationPackageUpdateExpireDateCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }


    [OpenApiOperation("", "خرید پکیج")]
    public async Task<Results<NoContent, BadRequest>> BuyPack(ISender sender, OrglocationPackageBuyPackCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "نمایش کامل لیست پکیج های فعال شرکت")]
    public async Task<Ok<List<OrglocationPackageGetCurrentUserPacksDTO>>> GetCurrentUserPacks(ISender sender)
    {
        var result = await sender.Send(new OrglocationPackageGetCurrentUserPacksQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش کامل لیست پکیج های شرکت ها")]
    public async Task<Ok<List<OrglocationPackageFindAllDTO>>> OrglocationPackageFindAll(ISender sender)
    {
        var result = await sender.Send(new OrglocationPackageFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "نمایش صفحه بندی شده ی لیست پکیج های شرکت ها")]
    public async Task<Ok<PaginatedList<OrglocationPackageWithPaginationDTO>>> GetOrglocationPackageWithPagination(
        ISender sender, OrglocationPackageWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation("", "واکشی پکیج های شرکت با کلید")]
    public async Task<Ok<OrglocationPackageFindByIdDTO>> GetOrglocationPackageById(ISender sender, Guid Guid)
    {
        var Response = await sender.Send(new OrglocationPackageFindByIdQuery(Guid));
        return TypedResults.Ok(Response);
    }

    [OpenApiOperation("", "ثبت پکیج های شرکت جدید")]
    public async Task<Created<Guid>> CreateOrglocationPackage(ISender sender, CreateOrglocationPackageCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(OrglocationPackage)}/{Guid}", Guid);
    }

    [OpenApiOperation("", "ویرایش پکیج های شرکت")]
    public async Task<Results<NoContent, BadRequest>> UpdateOrglocationPackage(ISender sender,
        UpdateOrglocationPackageCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation("", "حذف پکیج های شرکت")]
    public async Task<NoContent> DeleteOrglocationPackage(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteOrglocationPackageCommand(Guid));

        return TypedResults.NoContent();
    }
}