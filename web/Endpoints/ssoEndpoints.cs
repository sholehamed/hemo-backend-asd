using Application.Common.Models;
using Application.Core.sso_core.Commands.Createsso;
using Application.Core.sso_core.Commands.Updatesso;
using Application.Core.sso_core.Commands.Deletesso;
using Application.Core.sso_core.Commands.ssoGetKey;
using Application.Core.sso_core.Commands.ssoSsoLogin;
using Application.Core.sso_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace Web.Endpoints;

public class sso : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            //.RequireAuthorization()
            .MapPost(SsoLogin, "SsoLogin")
         .MapPost(GetKey, "GetKey");
        
    }


    [OpenApiOperation("", "لاگین با sso")]
    public async Task<Ok<Token>> SsoLogin(ISender sender, ssoSsoLoginCommand command)
    {
        var key = await sender.Send(command);
        return TypedResults.Ok(key);
    }

    [OpenApiOperation("", "ثبت SSO جدید")]
    public async Task<Ok<string>> GetKey(ISender sender, ssoGetKeyCommand command)
    {
        var key = await sender.Send(command);
        return TypedResults.Ok(key);
    }
  
 
}