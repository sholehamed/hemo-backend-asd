
using Application.Common.Models;
using Application.Core.SystemSetting_core.Commands.CreateSystemSetting;
using Application.Core.SystemSetting_core.Commands.UpdateSystemSetting;
using Application.Core.SystemSetting_core.Commands.DeleteSystemSetting;
using Application.Core.SystemSetting_core.Commands.SystemSettingUpdateLoginSetting;
using Application.Core.SystemSetting_core.Commands.SystemSettingUpdateTokenInfo;
using Application.Core.SystemSetting_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;
public class SystemSetting : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            // .MapGet(SystemSettingFindAll,"FindAll")
            // .MapPost(GetSystemSettingWithPagination,"BindPaging")
            // .MapPost(CreateSystemSetting)
             .MapPut(SystemSettingUpdateLoginSetting,"SystemSettingUpdateLoginSetting")
            .MapPut(SystemSettingUpdateTokenInfo,"SystemSettingUpdateTokenInfo")

            .MapGet(GetSystemSettingById, "GetSystemSettingById")
 
              ;
    }

    [OpenApiOperation( "","ویرایش تنظیمات لاگین")]
    public async Task<Results<NoContent, BadRequest>> SystemSettingUpdateLoginSetting(ISender sender, SystemSettingUpdateLoginSettingCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }
    [OpenApiOperation( "","ویرایش تنظیمات لاگین")]
    public async Task<Results<NoContent, BadRequest>> SystemSettingUpdateTokenInfo(ISender sender, SystemSettingUpdateTokenInfoCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [OpenApiOperation( "","واکشی تنظیمات سیستم با کلید")]
    public async Task<Ok<SystemSettingFindByIdDTO>> GetSystemSettingById(ISender sender)
    {
       var Response=  await sender.Send(new SystemSettingFindByIdQuery());
       return TypedResults.Ok(Response);
    }

   

   

  
}

