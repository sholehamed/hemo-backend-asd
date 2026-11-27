
using Application.Common.Models;
using Application.Core.Discount_core.Commands.CreateDiscount;
using Application.Core.Discount_core.Commands.UpdateDiscount;
using Application.Core.Discount_core.Commands.DeleteDiscount;
using Application.Core.Discount_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class Discount : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(DiscountFindAll,"FindAll")
            .MapPost(GetDiscountWithPagination,"BindPaging")
            .MapPost(CreateDiscount)
            .MapPut(UpdateDiscount)
            .MapGet(GetDiscountById, "{Guid}")
            .MapDelete(DeleteDiscount, "{Guid}")
              ;
    }

    [OpenApiOperation( "","نمایش کامل لیست کد تخفیف ها")]
    public async Task<Ok<List<DiscountFindAllDTO>>> DiscountFindAll(ISender sender)
    {
        var result = await sender.Send(new DiscountFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست کد تخفیف ها")]
    public async Task<Ok<PaginatedList<DiscountWithPaginationDTO>>> GetDiscountWithPagination(ISender sender,  DiscountWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی کد تخفیف با کلید")]
    public async Task<Ok<DiscountFindByIdDTO>> GetDiscountById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new DiscountFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت کد تخفیف جدید")]
    public async Task<Created<Guid>> CreateDiscount(ISender sender, CreateDiscountCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Discount)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش کد تخفیف")]
    public async Task<Results<NoContent, BadRequest>> UpdateDiscount(ISender sender, UpdateDiscountCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف کد تخفیف")]
    public async Task<NoContent> DeleteDiscount(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteDiscountCommand(Guid));

        return TypedResults.NoContent();
    }
}

