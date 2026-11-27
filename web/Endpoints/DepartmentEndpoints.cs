
using Application.Common.Models;
using Application.Core.Department_core.Commands.CreateDepartment;
using Application.Core.Department_core.Commands.UpdateDepartment;
using Application.Core.Department_core.Commands.DeleteDepartment;
using Application.Core.Department_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class Department : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapGet(DepartmentFindAll,"FindAll")
            .MapPost(GetDepartmentWithPagination,"BindPaging")
            .MapPost(CreateDepartment)
            .MapPut(UpdateDepartment)
            .MapGet(GetDepartmentById, "{Guid}")
            .MapDelete(DeleteDepartment, "{Guid}")
              ;
    }

    [OpenApiOperation( "","نمایش کامل لیست دپارتمان ها")]
    public async Task<Ok<List<DepartmentFindAllDTO>>> DepartmentFindAll(ISender sender)
    {
        var result = await sender.Send(new DepartmentFindAllQuery());
        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","نمایش صفحه بندی شده ی لیست دپارتمان ها")]
    public async Task<Ok<PaginatedList<DepartmentWithPaginationDTO>>> GetDepartmentWithPagination(ISender sender,  DepartmentWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    [OpenApiOperation( "","واکشی دپارتمان با کلید")]
    public async Task<Ok<DepartmentFindByIdDTO>> GetDepartmentById(ISender sender, Guid Guid)
    {
       var Response=  await sender.Send(new DepartmentFindByIdQuery(Guid));
       return TypedResults.Ok(Response);
    }

    [OpenApiOperation( "","ثبت دپارتمان جدید")]
    public async Task<Created<Guid>> CreateDepartment(ISender sender, CreateDepartmentCommand command)
    {
        var Guid = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Department)}/{Guid}", Guid);
    }

    [OpenApiOperation( "","ویرایش دپارتمان")]
    public async Task<Results<NoContent, BadRequest>> UpdateDepartment(ISender sender, UpdateDepartmentCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","حذف دپارتمان")]
    public async Task<NoContent> DeleteDepartment(ISender sender, Guid Guid)
    {
        await sender.Send(new DeleteDepartmentCommand(Guid));

        return TypedResults.NoContent();
    }
}

